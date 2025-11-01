using AutoFixture;
using AutoFixture.Xunit2;
using Moq;
using Xunit;

namespace MobileBackend.UnitTests;

/// <summary>
/// Base class for all unit tests providing common testing infrastructure
/// </summary>
public abstract class TestBase
{
    /// <summary>
    /// AutoFixture instance for generating test data
    /// </summary>
    protected readonly IFixture Fixture;
    
    /// <summary>
    /// Mock repository for creating and managing mocks
    /// </summary>
    protected readonly MockRepository MockRepository;

    protected TestBase()
    {
        Fixture = new Fixture();
        // Changed from Strict to Loose to prevent mock setup errors
        MockRepository = new MockRepository(MockBehavior.Loose);
        
        // Configure AutoFixture to omit recursion
        Fixture.Behaviors.OfType<ThrowingRecursionBehavior>().ToList()
            .ForEach(b => Fixture.Behaviors.Remove(b));
        Fixture.Behaviors.Add(new OmitOnRecursionBehavior());
    }

    /// <summary>
    /// Creates a mock object of type T with Loose behavior
    /// This allows methods to be called without explicit setup
    /// </summary>
    protected Mock<T> CreateMock<T>() where T : class
    {
        return MockRepository.Create<T>();
    }

    /// <summary>
    /// Verifies all mocks created by this test
    /// </summary>
    protected void VerifyAll()
    {
        MockRepository.VerifyAll();
    }

    /// <summary>
    /// Verifies a specific mock
    /// </summary>
    protected void Verify<T>(Mock<T> mock) where T : class
    {
        mock.Verify();
    }
}
