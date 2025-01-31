using TaskList;

namespace Tasks
{
	[TestFixture]
	public sealed class ApplicationTest
	{
		public const string PROMPT = "> ";

		private FakeConsole console;
		private System.Threading.Thread applicationThread;

		[SetUp]
		public void StartTheApplication()
		{
			this.console = new FakeConsole();
			var taskList = new TaskList.TaskList(console);
			this.applicationThread = new System.Threading.Thread(() => taskList.Run());
			applicationThread.Start();
			ReadLines(TaskList.TaskList.startupText);
		}

		[TearDown]
		public void KillTheApplication()
		{
			if (applicationThread == null || !applicationThread.IsAlive)
			{
				return;
			}

			applicationThread.Abort();
			throw new Exception("The application is still running.");
		}

		[Test, Timeout(1000)]
		public void ItWorks()
		{
			Execute("show");

			Execute("add project secrets");
			Execute("add task secrets Eat more donuts.");
			Execute("add task secrets Destroy all humans.");

			Execute("show");
			ReadLines(
				"secrets",
				"    [ ] 1: Eat more donuts.",
				"    [ ] 2: Destroy all humans.",
				""
			);

			Execute("add project training");
			Execute("add task training Four Elements of Simple Design");
			Execute("add task training SOLID");
			Execute("add task training Coupling and Cohesion");
			Execute("add task training Primitive Obsession");
			Execute("add task training Outside-In TDD");
			Execute("add task training Interaction-Driven Design");

			Execute("check 1");
			Execute("check 3");
			Execute("check 5");
			Execute("check 6");

			Execute("show");
			ReadLines(
				"secrets",
				"    [x] 1: Eat more donuts.",
				"    [ ] 2: Destroy all humans.",
				"",
				"training",
				"    [x] 3: Four Elements of Simple Design",
				"    [ ] 4: SOLID",
				"    [x] 5: Coupling and Cohesion",
				"    [x] 6: Primitive Obsession",
				"    [ ] 7: Outside-In TDD",
				"    [ ] 8: Interaction-Driven Design",
				""
			);

			Execute($"deadline 1 {DateTime.Today}");
			Execute($"deadline 2 {DateTime.Today.AddDays(-1)}");
			Execute($"deadline 3 {DateTime.Today}");
			Execute($"deadline 4 {DateTime.Today.AddDays(1)}");

			Execute("show");
			ReadLines(
				"secrets",
				$"    [x] 1: Eat more donuts. {DateTime.Today}",
				$"    [ ] 2: Destroy all humans. {DateTime.Today.AddDays(-1)}",
				"",
				"training",
				$"    [x] 3: Four Elements of Simple Design {DateTime.Today}",
				$"    [ ] 4: SOLID {DateTime.Today.AddDays(1)}",
				"    [x] 5: Coupling and Cohesion",
				"    [x] 6: Primitive Obsession",
				"    [ ] 7: Outside-In TDD",
				"    [ ] 8: Interaction-Driven Design",
				""
			);

			Execute("today");
			ReadLines(
				"secrets",
				$"    [x] 1: Eat more donuts. {DateTime.Today}",
				"",
				"training",
				$"    [x] 3: Four Elements of Simple Design {DateTime.Today}",
				""
			);

            Execute("view-by-deadline");
			ReadLines(
                $"{DateTime.Today.AddDays(-1).ToShortDateString()}:",
                "    secrets:",
                $"        2: Destroy all humans.",
				"",
                $"{DateTime.Today.ToShortDateString()}:",
                "    secrets:",
				$"        1: Eat more donuts.",
				"    training:",
				$"        3: Four Elements of Simple Design",
				"",
				$"{DateTime.Today.AddDays(1).ToShortDateString()}:",
				"    training:",
				$"        4: SOLID",
				"",
                "No deadline:",
                "    training:",
                "        5: Coupling and Cohesion",
                "        6: Primitive Obsession",
                "        7: Outside-In TDD",
                "        8: Interaction-Driven Design",
                ""
                );

            Execute("quit");
		}

		private void Execute(string command)
		{
			Read(PROMPT);
			Write(command);
		}

		private void Read(string expectedOutput)
		{
			var length = expectedOutput.Length;
			var actualOutput = console.RetrieveOutput(expectedOutput.Length);
			Assert.AreEqual(expectedOutput, actualOutput);
		}

		private void ReadLines(params string[] expectedOutput)
		{
			foreach (var line in expectedOutput)
			{
				Read(line + Environment.NewLine);
			}
		}

		private void Write(string input)
		{
			console.SendInput(input + Environment.NewLine);
		}
	}
}
