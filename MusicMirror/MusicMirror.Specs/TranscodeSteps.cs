using System;
using System.IO;
using TechTalk.SpecFlow;
using TechTalk.SpecFlow.Assist;
using TechTalk.SpecFlow.Async;

namespace MusicMirror.Specs
{
	[Binding]
	public class TranscodeSteps
	{
		private TestsContext _context
		{
			get
			{
				return ScenarioContext.Current.Get<TestsContext>("TestContext");
			}
		}

		public TranscodeSteps()
		{			
			ScenarioContext.Current.Add("TestContext", new TestsContext());
		}

		[Given(@"the source directory is ""(.*)""")]
		public void GivenTheSourceDirectoryIs(string sourceDirectory)
		{
			TestHelper.CreateTestFolder(sourceDirectory);
			_context.ViewModel.SourcePath.Value = TestHelper.GetFolderPath(sourceDirectory);
		}

		[Given(@"the target directory is ""(.*)""")]
		public void GivenTheTargetDirectoryIs(string targetDirectory)
		{
			TestHelper.CreateTestFolder(targetDirectory);
			_context.ViewModel.TargetPath.Value = TestHelper.GetFolderPath(targetDirectory);
		}

		[Given(@"the file ""(.*)"" is in the source path")]
		public void GivenTheFileIsTheSourcePath(string filename)
		{
			TestHelper.AddTestFileToFolder(filename, _context.ViewModel.SourcePath.Value);
		}

		[When(@"I save the settings")]
		public void WhenISaveTheSettings()
		{
			AsyncContext.Current.EnqueueDelay(TimeSpan.FromSeconds(5));
			_context.ViewModel.SaveCommand.Execute(null);			
		}

		[Then(@"the target path should contains the file ""(.*)""")]
		public void ThenTheTargetPathShouldContainsTheFile(string filename)
		{
			TestHelper.AssertFolderContainsFile(_context.ViewModel.TargetPath.Value, filename);
		}

		[Given(@"the file ""(.*)"" is in the source path subfolder ""(.*)""")]
		public void GivenTheFileIsTheSourcePathSubfolder(string filename, string subfoldername)
		{
			TestHelper.AddTestFileToFolder(filename, Path.Combine(_context.ViewModel.SourcePath.Value));
		}

		[Then(@"the target path with the subfolder ""(.*)"" should contains the file ""(.*)""")]
		public void ThenTheTargetPathShouldContainsTheFileInTheSubfolder(string subfolder, string filename)
		{
			TestHelper.AssertFolderContainsFile(Path.Combine(_context.ViewModel.TargetPath.Value, subfolder), filename);
		}

		[AfterScenario]
		public void Teardown()
		{
			_context.Dispose();
		}

	}
}
