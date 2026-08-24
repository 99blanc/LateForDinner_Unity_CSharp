using System;
using System.Collections.Generic;
using System.Linq;
using ZLinq;

public class ConsoleManager
{
    private readonly Dictionary<string, Action<string[]>> _commands = new Dictionary<string, Action<string[]>>(StringComparer.OrdinalIgnoreCase);
    private readonly Dictionary<string, string> _variables = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
    private readonly Dictionary<string, string> _commandDescriptions = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
    private readonly CommandRegistry _registry = new CommandRegistry();
    private readonly List<string> _histories = new List<string>();
    private int _index = -1;
    private const int _size = 50;

    public void Setup()
        => _registry.RegisterCommands(this);

    public void RegisterCommand(string command, Action<string[]> callback, string description = null)
    {
        string key = command.ToLower();
        _commands[key] = callback;
        _commandDescriptions[key] = string.IsNullOrEmpty(description) ? Managers.Localization.Get(LocalizationKey.Log_Console_NoDescription) : description;
    }

    public void UnregisterCommand(string command)
    {
        string key = command.ToLower();
        _commands.Remove(key);
        _commandDescriptions.Remove(key);
    }

    public IEnumerable<string> GetCommandNames()
        => _commands.Keys;

    public string GetDescription(string command)
        => _commandDescriptions.TryGetValue(command, out var desc) ? desc : string.Empty;

    public void SetVariable(string name, string value)
        => _variables[name.ToLower()] = value;

    public string GetVariable(string name)
        => _variables.TryGetValue(name.ToLower(), out var val) ? val : null;

    public List<string> GetAutoCompleteCandidates(string input)
    {
        if (string.IsNullOrWhiteSpace(input))
            return new List<string>();

        string lowerInput = input.ToLower();
        return _commands.Keys
        .Where(cmd => cmd.StartsWith(lowerInput))
        .ToList();
    }

    public void ProcessCommand(string inputCommand)
    {
        if (string.IsNullOrWhiteSpace(inputCommand))
            return;

        Log.Info(LocalizationKey.Console_Input, inputCommand);
        PushHistory(inputCommand);

        var parts = inputCommand.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
        if (parts.Length == 0)
            return;

        string cmdName = parts[0].ToLower();
        var args = new string[parts.Length - 1];
        Array.Copy(parts, 1, args, 0, args.Length); 

        if (_commands.TryGetValue(cmdName, out var action))
        {
            try
            {
                action.Invoke(args);
            }
            catch (Exception ex)
            {
                Log.Error(LocalizationKey.Log_Console_Error, $"{cmdName} - {ex.Message}");
            }
        }
        else
            Log.Warning(LocalizationKey.Log_Console_UnknownCommand, cmdName);
    }

    public string GetPreviousCommand()
    {
        if (_histories.Count == 0)
            return string.Empty;

        _index = Math.Max(0, _index - 1);
        return _histories[_index];
    }

    public string GetNextCommand()
    {
        if (_histories.Count == 0)
            return string.Empty;

        _index++;

        if (_index >= _histories.Count)
        {
            _index = _histories.Count;
            return string.Empty;
        }

        return _histories[_index];
    }

    private void PushHistory(string inputCommand)
    {
        if (_histories.Count == 0 || _histories[^1] != inputCommand)
        {
            _histories.Add(inputCommand);

            if (_histories.Count > _size)
                _histories.RemoveAt(0);
        }

        _index = _histories.Count;
    }
}
