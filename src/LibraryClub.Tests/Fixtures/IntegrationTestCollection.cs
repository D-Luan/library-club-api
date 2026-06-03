namespace LibraryClub.Tests.Fixtures;

[CollectionDefinition(Name, DisableParallelization = true)]
public class IntegrationTestCollection : ICollectionFixture<IntegrationTestFixture>
{
    public const string Name = "Integration Tests";
}