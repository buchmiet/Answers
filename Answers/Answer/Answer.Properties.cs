

namespace Answers;

public partial class Answer
{
    private const string ErrorAlreadyHasValue = "Answer already has value, therefore it cannot be in error state.";
    private const string ErrorCanOnlyBeSetOnce = "Error can only be set once.";
    private const string ValueNotSet = "Value was not set.";
    private const string InvalidMergeWithErrorState = "This object already has value ({0}) of type {1}, therefore it cannot be merged with another object in an error state.";
    private const string InvalidMergeWithAnotherValue = "There is already value ({0}) of type {1} assigned to this Answer object. You cannot merge value {2} of Type {3} from {4} with it.";
    private const string ValueIncorrectType = "Value is not of the correct type.";
    private const string ErrorStateNoValues = "Answer is in Error state, no values can be added.";
    protected AnswerState State = new();
    protected readonly MessageAggregator Messages = new();
    protected IAnswerValue AnswerValue;
    public bool IsSuccess => State.IsSuccess;
    public string Message => Messages.Message;
    public bool HasValue => AnswerValue is not null;
    public bool DialogConcluded
    {
        get => State.DialogConcluded;
        set => State.DialogConcluded = value;
    }
}