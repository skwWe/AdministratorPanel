using AdministratorPanel.Infrastructure.LogCollector.Repositories;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace AdministratorPanel.Tests.Infrastructure;

[TestClass]
public class DefaultServerGroupRepositoryTests
{
    [TestMethod]
    public void GetAll_Should_Return_Groups_Sorted_By_SortOrder_Then_Name()
    {
        var repository = new DefaultServerGroupRepository();

        var groups = repository.GetAll().ToArray();

        Assert.IsTrue(groups.Length > 0);

        for (var i = 1; i < groups.Length; i++)
        {
            var previous = groups[i - 1];
            var current = groups[i];

            var isSorted = previous.SortOrder < current.SortOrder ||
                           previous.SortOrder == current.SortOrder &&
                           string.Compare(previous.Name, current.Name, StringComparison.Ordinal) <= 0;

            Assert.IsTrue(isSorted, $"Groups are not sorted at index {i}.");
        }
    }

    [TestMethod]
    public void GetByName_Should_Return_Null_When_Value_Is_Null_Or_Whitespace()
    {
        var repository = new DefaultServerGroupRepository();

        Assert.IsNull(repository.GetByName(null!));
        Assert.IsNull(repository.GetByName(string.Empty));
        Assert.IsNull(repository.GetByName("   "));
    }

    [TestMethod]
    public void GetByName_Should_Return_Null_When_Group_Does_Not_Exist()
    {
        var repository = new DefaultServerGroupRepository();

        var result = repository.GetByName("missing-group-name");

        Assert.IsNull(result);
    }
}
