using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Versioning
{
	public static class ProcessExtensions
	{
		/// <summary>
		/// Waits asynchronously for the process to exit.
		/// </summary>
		/// <param name="process">The process to wait for cancellation.</param>
		/// <param name="cancellationToken">A cancellation token. If invoked, the task will return immediately as canceled.</param>
		/// <returns>A Task representing waiting for the process to end.</returns>
		public static Task<int> WaitForExitAsync(this Process process, CancellationToken cancellationToken = default)
		{
			var tcs = new TaskCompletionSource<int>();
			process.EnableRaisingEvents = true;
			process.Exited += (sender, args) => tcs.TrySetResult(process.ExitCode);
			if (cancellationToken != default)
				cancellationToken.Register(tcs.SetCanceled);

			return tcs.Task;
		}
		/// <summary>
		/// Normally when a processes is started from C#, it is a child process of the calling process. In the case the calling process exists, the child process is also terminated.
		/// This method starts a process without this relaton with the calling process, so that new process can outlive it.
		/// </summary>
		/// <returns>A Task representing waiting for the process to end.</returns>
		public static Task<int> StartIndependentlyAsync(string executable, params string[] arguments)
		{
			return startIndependentlyAsync(executable, visibly: true, arguments: arguments);
		}
		/// <summary>
		/// Normally when a processes is started from C#, it is a child process of the calling process. In the case the calling process exists, the child process is also terminated.
		/// This method starts a process without this relaton with the calling process, so that new process can outlive it, and starts it without showing the cmd.
		/// </summary>
		/// <returns>A Task representing waiting for the process to end.</returns>
		public static Task<int> StartIndependentlyInvisiblyAsync(string executable, params string[] arguments)
		{
			return startIndependentlyAsync(executable, visibly: false, arguments: arguments);
		}

		private static Task<int> startIndependentlyAsync(string executable, bool visibly, params string[] arguments)
		{
			if (string.IsNullOrEmpty(executable)) throw new ArgumentNullException(nameof(executable));
			if (!executable.EndsWith(".exe")) throw new ArgumentException();
			if (!File.Exists(executable)) throw new ArgumentException(); // thrown exception should propagate
			if (arguments == null) throw new ArgumentNullException(nameof(arguments));
			if (arguments.Any(string.IsNullOrEmpty)) throw new ArgumentException(nameof(arguments));

			var info = new ProcessStartInfo(executable, string.Join(" ", arguments));
			if (!visibly)
			{
				info.CreateNoWindow = true;
				info.WindowStyle = ProcessWindowStyle.Hidden;
			}
			return info.StartIndependentlyAsync();
		}

		/// <summary>
		/// Normally when a processes is started from C#, it is a child process of the calling process. In the case the calling process exists, the child process is also terminated.
		/// This method starts a process without ths relaton with the calling process, so that new process can outlive it.
		/// </summary>
		/// <returns>A Task representing waiting for the process to end.</returns>
		public static Task<int> StartIndependentlyAsync(this ProcessStartInfo startInfo)
		{
			return Process.Start(startInfo).WaitForExitAsync();
		}
	}
}
