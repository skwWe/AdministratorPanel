using AdministratorPanel.Modules.LogCollector.Abstractions;
using AdministratorPanel.Modules.LogCollector.Enums;
using AdministratorPanel.Modules.LogCollector.Models;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text;

namespace AdministratorPanel.Infrastructure.LogCollector.Services;

public sealed class BashLogScriptRunner : ILogScriptRunner
{
    public async Task<LogScriptExecutionResult> RunAsync(
        LogCollectionGroupType groupType,
        string sshUserName,
        string? password,
        IProgress<LogScriptProgressMessage>? progress = null,
        CancellationToken cancellationToken = default)
    {
        var scriptPath = ResolveScriptPath(groupType);

        if (!File.Exists(scriptPath))
        {
            return new LogScriptExecutionResult
            {
                IsSuccess = false,
                ExitCode = -1,
                ExecutedScriptPath = scriptPath,
                StandardError = $"Скрипт не найден: {scriptPath}"
            };
        }

        var processStartInfo = BuildProcessStartInfo(scriptPath, sshUserName, password);

        using var process = new Process
        {
            StartInfo = processStartInfo,
            EnableRaisingEvents = true
        };

        var stdoutBuilder = new StringBuilder();
        var stderrBuilder = new StringBuilder();

        var stdoutCompletion = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
        var stderrCompletion = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);

        process.OutputDataReceived += (_, args) =>
        {
            if (args.Data is null)
            {
                stdoutCompletion.TrySetResult(true);
                return;
            }

            stdoutBuilder.AppendLine(args.Data);
            progress?.Report(new LogScriptProgressMessage
            {
                IsError = false,
                Text = args.Data
            });
        };

        process.ErrorDataReceived += (_, args) =>
        {
            if (args.Data is null)
            {
                stderrCompletion.TrySetResult(true);
                return;
            }

            stderrBuilder.AppendLine(args.Data);
            progress?.Report(new LogScriptProgressMessage
            {
                IsError = true,
                Text = args.Data
            });
        };

        try
        {
            if (!process.Start())
            {
                return new LogScriptExecutionResult
                {
                    IsSuccess = false,
                    ExitCode = -2,
                    ExecutedScriptPath = scriptPath,
                    StandardError = "Не удалось запустить процесс."
                };
            }

            process.BeginOutputReadLine();
            process.BeginErrorReadLine();

            await process.WaitForExitAsync(cancellationToken);
            await Task.WhenAll(stdoutCompletion.Task, stderrCompletion.Task);

            return new LogScriptExecutionResult
            {
                IsSuccess = process.ExitCode == 0,
                ExitCode = process.ExitCode,
                StandardOutput = stdoutBuilder.ToString(),
                StandardError = stderrBuilder.ToString(),
                ExecutedScriptPath = scriptPath
            };
        }
        catch (OperationCanceledException)
        {
            TryTerminateProcess(process);

            return new LogScriptExecutionResult
            {
                IsSuccess = false,
                ExitCode = -3,
                ExecutedScriptPath = scriptPath,
                StandardError = "Выполнение скрипта отменено."
            };
        }
        catch (Exception ex)
        {
            TryTerminateProcess(process);

            return new LogScriptExecutionResult
            {
                IsSuccess = false,
                ExitCode = -4,
                ExecutedScriptPath = scriptPath,
                StandardError = $"Ошибка запуска скрипта: {ex.Message}"
            };
        }
    }

    private static string EscapeForSingleQuotedShell(string value)
    {
        return value.Replace("'", "'\"'\"'");
    }

    private static string EscapeForDoubleQuotedWindowsArgument(string value)
    {
        return value.Replace("\"", "\\\"");
    }

    private static ProcessStartInfo BuildProcessStartInfo(string scriptPath, string sshUserName, string? password)
    {
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            var wslPath = ConvertWindowsPathToWslPath(scriptPath);

            var escapedUser = EscapeForSingleQuotedShell(sshUserName);
            var escapedPassword = EscapeForSingleQuotedShell(password ?? string.Empty);

            var linuxCommand =
                $"export SSH_USER_NAME='{escapedUser}'; " +
                $"export SSH_PASS='{escapedPassword}'; " +
                $"export SUDO_PASS='{escapedPassword}'; " +
                $"bash '{wslPath}'";

            var psi = new ProcessStartInfo
            {
                FileName = "wsl.exe",
                Arguments = $"sh -lc \"{EscapeForDoubleQuotedWindowsArgument(linuxCommand)}\"",
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                RedirectStandardInput = false,
                UseShellExecute = false,
                CreateNoWindow = true,
                WorkingDirectory = AppContext.BaseDirectory,
                StandardOutputEncoding = Encoding.UTF8,
                StandardErrorEncoding = Encoding.UTF8
            };

            Console.WriteLine($"WSL script path: {wslPath}");
            Console.WriteLine($"SSH_USER_NAME set: {!string.IsNullOrWhiteSpace(sshUserName)}");
            Console.WriteLine($"PASSWORD EMPTY: {string.IsNullOrWhiteSpace(password)}");

            return psi;
        }

        var linuxPsi = new ProcessStartInfo
        {
            FileName = "/bin/bash",
            Arguments = $"\"{scriptPath}\"",
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            RedirectStandardInput = false,
            UseShellExecute = false,
            CreateNoWindow = true,
            WorkingDirectory = Path.GetDirectoryName(scriptPath) ?? AppContext.BaseDirectory,
            StandardOutputEncoding = Encoding.UTF8,
            StandardErrorEncoding = Encoding.UTF8
        };

        if (!string.IsNullOrWhiteSpace(sshUserName))
        {
            linuxPsi.Environment["SSH_USER_NAME"] = sshUserName;
        }

        if (!string.IsNullOrWhiteSpace(password))
        {
            linuxPsi.Environment["SSH_PASS"] = password;
            linuxPsi.Environment["SUDO_PASS"] = password;
        }

        return linuxPsi;
    }

    private static string ResolveScriptPath(LogCollectionGroupType groupType)
    {
        var scriptFileName = groupType switch
        {
            LogCollectionGroupType.App => "collect_app_logs.sh",
            LogCollectionGroupType.Convert => "collect_convert_logs.sh",
            LogCollectionGroupType.Sync => "collect_sync_logs.sh",
            LogCollectionGroupType.Web => "collect_web_logs.sh",
            _ => throw new ArgumentOutOfRangeException(nameof(groupType), groupType, "Неизвестная группа логов.")
        };

        return Path.Combine(AppContext.BaseDirectory, "Assets", "Scripts", scriptFileName);
    }

    private static string ConvertWindowsPathToWslPath(string windowsPath)
    {
        var fullPath = Path.GetFullPath(windowsPath).Replace('\\', '/');

        if (fullPath.Length < 2 || fullPath[1] != ':')
        {
            throw new InvalidOperationException($"Невозможно преобразовать путь в WSL-формат: {windowsPath}");
        }

        var driveLetter = char.ToLowerInvariant(fullPath[0]);
        var pathWithoutDrive = fullPath[2..];

        return $"/mnt/{driveLetter}{pathWithoutDrive}";
    }

    private static void TryTerminateProcess(Process process)
    {
        try
        {
            if (!process.HasExited)
            {
                process.Kill(entireProcessTree: true);
            }
        }
        catch
        {
        }
    }
}