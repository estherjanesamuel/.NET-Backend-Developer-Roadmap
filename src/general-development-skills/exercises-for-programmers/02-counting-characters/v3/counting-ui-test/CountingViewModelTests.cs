using counting_ui;

namespace counting_ui_test;

public class Tests
{
    [Test]
    public void Count_OnInputChange_IsUpdated()
    {
        var vm = new CountingViewModel();
        int want = 5;
        vm.Input = "Hello";
        Assert.That(vm.Count, Is.EqualTo(want));
    }

    [Test]
    public void Count_OnEmptyInput_IsZero()
    {
        var vm = new CountingViewModel();
        int want = 0;
        vm.Input = "";
        Assert.That(vm.Count, Is.EqualTo(want));
    }

    [Test]
    public void Count_OnNullInput_IsZero()
    {
        var vm = new CountingViewModel();
        int want = 0;
        vm.Input = null;
        Assert.That(vm.Count, Is.EqualTo(want));
    }

    [Test]
    public void Message_OnInputChange_IsUpdated()
    {
        var vm = new CountingViewModel();
        string want = "Hello has 5 characters.";
        vm.Input = "Hello";
        Assert.That(vm.Message, Is.EqualTo(want));
    }

    [Test]
    public void Message_OnEmptyInput_IsEmpty()
    {
        var vm = new CountingViewModel();
        string want = "";
        vm.Input = "";
        Assert.That(vm.Message, Is.EqualTo(want));
    }

    [Test]
    public void Message_OnNullInput_IsEmpty()
    {
        var vm = new CountingViewModel();
        string want = "";
        vm.Input = null;
        Assert.That(vm.Message, Is.EqualTo(want));
    }
}