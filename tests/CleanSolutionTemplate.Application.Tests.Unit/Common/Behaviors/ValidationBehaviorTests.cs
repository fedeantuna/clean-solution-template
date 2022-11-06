using CleanSolutionTemplate.Application.Common.Behaviors;
using FluentAssertions;
using FluentValidation;
using FluentValidation.Results;
using MediatR;
using Moq;

namespace CleanSolutionTemplate.Application.Tests.Unit.Common.Behaviors;

public class ValidationBehaviorTests : TestBase
{
    private const string HandlerResponse = "test-handler-response";

    private readonly Mock<IRequest<string>> _requestMock;

    private readonly ValidationBehavior<IRequest<string>, string> _sut;

    public ValidationBehaviorTests()
    {
        this._requestMock = new Mock<IRequest<string>>();

        var pipelineBehaviors = this.FindService<IEnumerable<IPipelineBehavior<IRequest<string>, string>>>();
        this._sut = (ValidationBehavior<IRequest<string>, string>)pipelineBehaviors.First(pb =>
            pb.GetType().Name == typeof(ValidationBehavior<,>).Name);
    }

    [Fact]
    public async Task Handle_CallsValidateAsyncOnEachValidatorAndReturnsRequestHandlerResult_WhenValidationsAreSuccessful()
    {
        // Arrange
        SetupPassingValidator(this.ValidatorAMock, this._requestMock.Object);
        SetupPassingValidator(this.ValidatorBMock, this._requestMock.Object);

        // Act
        var result = await this._sut.Handle(this._requestMock.Object, Handler, default);

        // Assert
        VerifyValidation(this.ValidatorAMock, this._requestMock.Object);
        VerifyValidation(this.ValidatorBMock, this._requestMock.Object);
        result.Should().Be(HandlerResponse);
    }

    [Fact]
    public async Task Handle_CallsValidateAsyncOnEachValidatorAndThrowsValidationException_WhenValidationsContainFailures()
    {
        // Arrange
        SetupPassingValidator(this.ValidatorBMock, this._requestMock.Object);
        SetupFailingValidator(this.ValidatorAMock, this._requestMock.Object);

        // Act
        Func<Task> act = () => this._sut.Handle(this._requestMock.Object, Handler, default);

        // Assert
        await act.Should().ThrowAsync<ValidationException>();
        VerifyValidation(this.ValidatorBMock, this._requestMock.Object);
        VerifyValidation(this.ValidatorAMock, this._requestMock.Object);
    }

    [Fact]
    public async Task Handle_ReturnsRequestHandlerResult_WhenNoValidatorsAreRegistered()
    {
        // Arrange
        var sut = new ValidationBehavior<IRequest<string>, string>(Enumerable.Empty<IValidator<IRequest<string>>>());

        // Act
        var result = await sut.Handle(this._requestMock.Object, Handler, default);

        // Assert
        result.Should().Be(HandlerResponse);
    }

    private static Task<string> Handler() =>
        Task.FromResult(HandlerResponse);

    private static void SetupPassingValidator(Mock<IValidator<IRequest<string>>> validator, IRequest<string> request)
    {
        validator.Setup(v =>
            v.ValidateAsync(It.Is<ValidationContext<IRequest<string>>>(vc =>
                    vc.InstanceToValidate == request),
                default)).ReturnsAsync(new ValidationResult());
    }

    private static void SetupFailingValidator(Mock<IValidator<IRequest<string>>> validator, IRequest<string> request)
    {
        var validationFailures = new List<ValidationFailure>
        {
            new("test-property", "test-error-message")
        };

        validator.Setup(v =>
            v.ValidateAsync(It.Is<ValidationContext<IRequest<string>>>(vc =>
                    vc.InstanceToValidate == request),
                default)).ReturnsAsync(new ValidationResult(validationFailures));
    }

    private static void VerifyValidation(Mock<IValidator<IRequest<string>>> validatorMock, IRequest<string> request)
    {
        validatorMock.Verify(v =>
            v.ValidateAsync(It.Is<ValidationContext<IRequest<string>>>(vc =>
                    vc.InstanceToValidate == request),
                default), Times.Once);
    }
}
