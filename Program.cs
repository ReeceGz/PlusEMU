﻿using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using NLog;
using NLog.Extensions.Logging;
using Plus.Core;
using System.Reflection;
using Microsoft.Extensions.Logging;
using Plus.Communication.Flash;
using Plus.Communication.Nitro;
using Plus.Communication.RCON;
using Plus.Database;
using Plus.Plugins;
using Plus.Utilities.DependencyInjection;
using Scrutor;

namespace Plus;

public static class Program
{
    private static Dictionary<ServiceLifetime, IEnumerable<Type>> _defaultTypes = new();
    private static IPlusEnvironment _environment;
    private static IConsoleCommandHandler _consoleCommands;

    public static async Task Main(string[] args)
    {
        Dapper.DefaultTypeMap.MatchNamesWithUnderscores = true;
        
        var services = new ServiceCollection();
        _defaultTypes[ServiceLifetime.Singleton] = typeof(Program).Assembly.GetTypes().Where(t => t.IsInterface && t.GetCustomAttributes<SingletonAttribute>().Any());
        _defaultTypes[ServiceLifetime.Scoped] = typeof(Program).Assembly.GetTypes().Where(t => t.IsInterface && t.GetCustomAttributes<ScopedAttribute>().Any());

        // Plugins
        Directory.CreateDirectory("plugins");
        var pluginAssemblies = new DirectoryInfo("plugins").GetDirectories().Select(d => PluginLoadContext.LoadPlugin(Path.Join("plugins", d.Name), d.Name)).ToList();
        pluginAssemblies.AddRange(new DirectoryInfo("plugins").GetFiles().Where(f => Path.GetExtension(f.Name).Equals(".dll")).Select(f => PluginLoadContext.LoadPlugin(Path.Join("plugins"), Path.GetFileNameWithoutExtension(f.Name))));
        var pluginDefinitions = pluginAssemblies.SelectMany(pluginAssembly => AddPlugin(services, pluginAssembly)).ToList();

        // Configuration
        var configuration = new ConfigurationBuilder()
            .SetBasePath(Path.Join(Directory.GetCurrentDirectory(), "Config"))
            .AddJsonFile("config.json")
            .Build();

        services.AddConfiguration<FlashServerConfiguration>(configuration.GetSection("Flash"));
        services.AddConfiguration<NitroServerConfiguration>(configuration.GetSection("Nitro"));
        services.AddConfiguration<DatabaseConfiguration>(configuration.GetSection("Database"));
        services.AddConfiguration<RconConfiguration>(configuration.GetSection("Rcon"));

        // Dependency Injection
        services.AddDefaultRules(typeof(Program).Assembly);
        services.AddSingleton<IPlusEnvironment, PlusEnvironment>();

        foreach (var plugin in pluginDefinitions)
            plugin.OnServicesConfigured();


        // Configuration
        LogManager.LoadConfiguration(Path.Join(Directory.GetCurrentDirectory(), "Config", "nlog.config"));
        services.AddLogging(loggingBuilder =>
        {
            loggingBuilder.ClearProviders();
            loggingBuilder.SetMinimumLevel(Microsoft.Extensions.Logging.LogLevel.Trace);
            loggingBuilder.AddNLog();
        });

        var serviceProvider = services.BuildServiceProvider();
        foreach (var plugin in pluginDefinitions)
            plugin.OnServiceProviderBuild(serviceProvider);

        _environment = serviceProvider.GetRequiredService<IPlusEnvironment>();
        _consoleCommands = serviceProvider.GetRequiredService<IConsoleCommandHandler>();

        Console.ForegroundColor = ConsoleColor.White;
        Console.CursorVisible = false;
        AppDomain.CurrentDomain.UnhandledException += OnUnhandledException;

        // Start
        var started = await _environment.Start();
        if (!started)
        {
            Environment.Exit(1);
            return;
        }
        while (true)
        {
            if (Console.ReadKey(true).Key == ConsoleKey.Enter)
            {
                Console.Write("plus> ");
                var input = Console.ReadLine();
                if (input.Length > 0)
                    _consoleCommands.InvokeCommand(input);
            }
        }
    }

    public static IServiceCollection AddAssignableTo<T>(this IServiceCollection services, Assembly assembly, ServiceLifetime lifetime = ServiceLifetime.Singleton) => services.AddAssignableTo(new[] { assembly }, typeof(T), lifetime);

    public static IServiceCollection AddAssignableTo<T>(this IServiceCollection services, IEnumerable<Assembly> assemblies, ServiceLifetime lifetime = ServiceLifetime.Singleton) => services.AddAssignableTo(assemblies, typeof(T), lifetime);

    public static IServiceCollection AddAssignableTo(this IServiceCollection services, Assembly assembly, Type type, ServiceLifetime lifetime = ServiceLifetime.Singleton) => services.AddAssignableTo(new[] { assembly }, type, lifetime);

    public static IServiceCollection AddAssignableTo(this IServiceCollection services, IEnumerable<Assembly> assemblies, Type type, ServiceLifetime lifetime = ServiceLifetime.Singleton) =>
        services.Scan(scan => scan.FromAssemblies(assemblies)
            .AddClasses(classes => classes.Where(t => t.IsAssignableTo(type) && !t.IsAbstract && !t.IsInterface))
            .UsingRegistrationStrategy(RegistrationStrategy.Append)
            .AsSelfWithInterfaces()
            .WithSingletonLifetime());

    private static IServiceCollection AddDefaultRules(this IServiceCollection services, Assembly assembly)
    {
        foreach (var type in assembly.GetTypes().Where(t => t.IsInterface && t.GetCustomAttributes<SingletonAttribute>().Any()).Concat(_defaultTypes[ServiceLifetime.Singleton]).Distinct())
            services.AddAssignableTo(assembly, type, ServiceLifetime.Singleton);
        foreach (var type in assembly.GetTypes().Where(t => t.IsInterface && t.GetCustomAttributes<ScopedAttribute>().Any()).Concat(_defaultTypes[ServiceLifetime.Scoped]).Distinct())
            services.AddAssignableTo(assembly, type, ServiceLifetime.Scoped);

        services.Scan(scan => scan.FromAssemblies(assembly)
            .AddClasses(classes => classes.Where(c => c.GetInterface($"I{c.Name}") != null))
            .UsingRegistrationStrategy(RegistrationStrategy.Skip)
            .AsSelfWithInterfaces()
            .WithSingletonLifetime());
        return services;
    }

    private static IEnumerable<IPluginDefinition> AddPlugin(IServiceCollection services, Assembly pluginAssembly)
    {
        var pluginDefinitions = new List<IPluginDefinition>();
        try
        {
            services.AddDefaultRules(pluginAssembly);

            foreach (var pluginDefinition in pluginAssembly.DefinedTypes.Where(t =>
                         t.ImplementedInterfaces.Contains(typeof(IPluginDefinition))))
            {
                var plugin = (IPluginDefinition?)Activator.CreateInstance(pluginDefinition);
                if (plugin != null)
                {
                    plugin.ConfigureServices(services);
                    services.AddSingleton(plugin);
                    pluginDefinitions.Add(plugin);
                }
            }
        }
        catch (Exception e)
        {
            Console.WriteLine($"Failed to load plugin assembly { pluginAssembly.FullName}. Possibly outdated. {e.Message}");
        }
        return pluginDefinitions;
    }
    public static IServiceCollection AddConfiguration<T>(this IServiceCollection services, IConfigurationSection section)
        where T : class
    {
        services.Configure<T>(section);
        return services;
    }

    private static void OnUnhandledException(object sender, UnhandledExceptionEventArgs args)
    {
        var e = (Exception)args.ExceptionObject;
        //Logger.LogCriticalException("SYSTEM CRITICAL EXCEPTION: " + e);
        _environment?.PerformShutDown();
    }
}