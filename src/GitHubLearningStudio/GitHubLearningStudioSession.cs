namespace TheSingularityWorkshop.Experiences.GitHubLearningStudio;

/// <summary>Stable semantic actions in the GitHub Pages learning simulation.</summary>
public static class GitHubLearningStudioCommands
{
    public const string InspectReadme = "project.inspect-readme";
    public const string CreateBranch = "git.create-branch";
    public const string EditReadme = "project.edit-readme";
    public const string CommitChanges = "git.commit";
    public const string ConfigurePages = "pages.configure";
    public const string BuildPages = "pages.build";
    public const string PublishPages = "pages.publish";
    public const string AnswerKnowledgeCheck = "lesson.answer-update-question";
}

/// <summary>Describes the modeled publication lifecycle.</summary>
public enum PagesLifecycle
{
    NotConfigured,
    Configured,
    Built,
    Published,
    UpdateAvailable
}

/// <summary>A recorded educational commit, not a real Git object.</summary>
public sealed record LearningCommit(
    int Sequence,
    string Branch,
    string Message,
    string ReadmeContent);

/// <summary>A complete immutable view of the current lesson simulation.</summary>
public sealed record GitHubLearningStudioSnapshot(
    string ExperienceId,
    string ProjectName,
    string RepositoryName,
    string ActiveBranch,
    bool ReadmeInspected,
    string ReadmeContent,
    bool HasUncommittedChanges,
    IReadOnlyList<LearningCommit> Commits,
    bool PagesConfigured,
    PagesLifecycle PagesLifecycle,
    int? BuiltCommitSequence,
    int? PublishedCommitSequence,
    string? SimulatedPublishedUrl,
    string Explanation,
    string GuidedPracticePrompt,
    string KnowledgeCheckPrompt,
    bool KnowledgeCheckCorrect,
    bool LessonCompleted,
    IReadOnlyList<string> AvailableCommands);

/// <summary>The result of one semantic lesson command.</summary>
public sealed record GitHubLearningStudioCommandResult(
    bool Accepted,
    string CommandId,
    string Message,
    GitHubLearningStudioSnapshot Snapshot);

/// <summary>
/// Deterministic, in-memory teaching model for a repository-to-Pages journey.
/// No Git commands, GitHub APIs, network requests, files, or real Pages configuration are performed.
/// </summary>
public sealed class GitHubLearningStudioSession
{
    private const string InitialReadme = """
        # Field Notes

        A tiny project for learning how to share work.

        This site will become a public home for project notes.
        """;

    private readonly List<LearningCommit> _commits = [];
    private string _readmeContent = InitialReadme;
    private string _activeBranch = "main";
    private bool _readmeInspected;
    private bool _hasUncommittedChanges;
    private bool _pagesConfigured;
    private PagesLifecycle _pagesLifecycle;
    private int? _builtCommitSequence;
    private int? _publishedCommitSequence;
    private bool _knowledgeCheckCorrect;

    public GitHubLearningStudioSession()
    {
        _commits.Add(new LearningCommit(1, "main", "Create the starter project", InitialReadme));
        _pagesLifecycle = PagesLifecycle.NotConfigured;
    }

    public GitHubLearningStudioSnapshot CaptureSnapshot()
    {
        var commands = new List<string>();
        if (!_readmeInspected)
            commands.Add(GitHubLearningStudioCommands.InspectReadme);
        else if (_activeBranch == "main")
            commands.Add(GitHubLearningStudioCommands.CreateBranch);
        else
            commands.Add(GitHubLearningStudioCommands.EditReadme);
        if (_hasUncommittedChanges)
            commands.Add(GitHubLearningStudioCommands.CommitChanges);
        if (!_pagesConfigured && !_hasUncommittedChanges && _commits.Count > 1)
            commands.Add(GitHubLearningStudioCommands.ConfigurePages);
        if (_pagesConfigured && _builtCommitSequence != _commits[^1].Sequence)
            commands.Add(GitHubLearningStudioCommands.BuildPages);
        if (_pagesConfigured && _builtCommitSequence == _commits[^1].Sequence &&
            _publishedCommitSequence != _builtCommitSequence)
            commands.Add(GitHubLearningStudioCommands.PublishPages);
        if (_publishedCommitSequence == _commits[^1].Sequence && !_knowledgeCheckCorrect)
            commands.Add(GitHubLearningStudioCommands.AnswerKnowledgeCheck);

        return new GitHubLearningStudioSnapshot(
            "github-learning-studio.pages-first",
            "Field Notes",
            "field-notes",
            _activeBranch,
            _readmeInspected,
            _readmeContent,
            _hasUncommittedChanges,
            Array.AsReadOnly(_commits.ToArray()),
            _pagesConfigured,
            _pagesLifecycle,
            _builtCommitSequence,
            _publishedCommitSequence,
            _publishedCommitSequence is null ? null : "https://example.invalid/field-notes/",
            BuildExplanation(),
            "Make one more small README improvement, commit it, and follow the build → publish path to update the modeled site.",
            "A new commit exists after the site was published. What must happen before visitors see that newer version?",
            _knowledgeCheckCorrect,
            _knowledgeCheckCorrect,
            Array.AsReadOnly(commands.ToArray()));
    }

    public GitHubLearningStudioCommandResult Execute(string commandId, string? value = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(commandId);

        var accepted = commandId switch
        {
            GitHubLearningStudioCommands.InspectReadme => InspectReadme(),
            GitHubLearningStudioCommands.CreateBranch => CreateBranch(),
            GitHubLearningStudioCommands.EditReadme => EditReadme(value),
            GitHubLearningStudioCommands.CommitChanges => CommitChanges(value),
            GitHubLearningStudioCommands.ConfigurePages => ConfigurePages(),
            GitHubLearningStudioCommands.BuildPages => BuildPages(),
            GitHubLearningStudioCommands.PublishPages => PublishPages(),
            GitHubLearningStudioCommands.AnswerKnowledgeCheck => AnswerKnowledgeCheck(value),
            _ => false
        };

        var message = commandId == GitHubLearningStudioCommands.AnswerKnowledgeCheck && accepted
            ? (_knowledgeCheckCorrect
                ? "Correct. A new commit must be built and then published before the modeled site reflects it."
                : "Not quite. A commit records the change; the site still needs a new build and publication.")
            : accepted
                ? $"The simulated action '{commandId}' was accepted."
                : $"The action '{commandId}' is not available in the current lesson state.";
        return new GitHubLearningStudioCommandResult(accepted, commandId, message, CaptureSnapshot());
    }

    private bool InspectReadme()
    {
        if (_readmeInspected) return false;
        _readmeInspected = true;
        return true;
    }

    private bool CreateBranch()
    {
        if (!_readmeInspected || _activeBranch != "main" || _hasUncommittedChanges)
            return false;
        _activeBranch = "work/improve-project-home";
        return true;
    }

    private bool EditReadme(string? value)
    {
        if (!_readmeInspected || _activeBranch == "main" || string.IsNullOrWhiteSpace(value))
            return false;
        if (string.Equals(value, _readmeContent, StringComparison.Ordinal))
            return false;

        _readmeContent = value.Trim();
        _hasUncommittedChanges = true;
        if (_pagesConfigured)
            _pagesLifecycle = PagesLifecycle.UpdateAvailable;
        return true;
    }

    private bool CommitChanges(string? value)
    {
        if (!_hasUncommittedChanges)
            return false;

        var message = string.IsNullOrWhiteSpace(value)
            ? "Update the project home"
            : value.Trim();
        _commits.Add(new LearningCommit(_commits.Count + 1, _activeBranch, message, _readmeContent));
        _hasUncommittedChanges = false;
        if (_pagesConfigured)
            _pagesLifecycle = PagesLifecycle.UpdateAvailable;
        return true;
    }

    private bool ConfigurePages()
    {
        if (_pagesConfigured || _hasUncommittedChanges || !_readmeInspected || _commits.Count < 2)
            return false;
        _pagesConfigured = true;
        _pagesLifecycle = PagesLifecycle.Configured;
        return true;
    }

    private bool BuildPages()
    {
        if (!_pagesConfigured || _hasUncommittedChanges ||
            _builtCommitSequence == _commits[^1].Sequence)
            return false;
        _builtCommitSequence = _commits[^1].Sequence;
        _pagesLifecycle = PagesLifecycle.Built;
        return true;
    }

    private bool PublishPages()
    {
        if (!_pagesConfigured || _builtCommitSequence != _commits[^1].Sequence ||
            _publishedCommitSequence == _builtCommitSequence)
            return false;
        _publishedCommitSequence = _builtCommitSequence;
        _pagesLifecycle = PagesLifecycle.Published;
        return true;
    }

    private bool AnswerKnowledgeCheck(string? value)
    {
        if (_publishedCommitSequence != _commits[^1].Sequence || _knowledgeCheckCorrect)
            return false;

        _knowledgeCheckCorrect = string.Equals(value?.Trim(), "build-then-publish", StringComparison.OrdinalIgnoreCase);
        return true;
    }

    private string BuildExplanation()
    {
        if (!_readmeInspected)
            return "Start with the project home: the README explains what the repository contains.";
        if (_activeBranch == "main")
            return "You have inspected the project. Create a branch to make an isolated change without rewriting the shared main line.";
        if (_hasUncommittedChanges)
            return "Your working copy has an edit that is not yet part of the recorded project history. Commit it when the change is ready.";
        if (!_pagesConfigured)
            return "The change is recorded in the branch. Configure the modeled Pages source to begin the publishing journey.";
        if (_pagesLifecycle == PagesLifecycle.Configured)
            return "The source is selected. A build turns the committed project content into a site artifact.";
        if (_pagesLifecycle == PagesLifecycle.Built)
            return "The site artifact is ready. Publishing makes this modeled version available at the example URL.";
        if (_pagesLifecycle == PagesLifecycle.UpdateAvailable)
            return "The published site is behind the latest commit. Build and publish again to model the update loop.";
        if (_knowledgeCheckCorrect)
            return "You have completed the first journey: a recorded change became a modeled published site, and you can explain how updates reach visitors.";
        return "The published site represents the recorded commit. Complete the short update question to check your understanding.";
    }
}
