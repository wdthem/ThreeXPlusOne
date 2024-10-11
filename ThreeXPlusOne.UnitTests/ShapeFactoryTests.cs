using FluentAssertions;
using ThreeXPlusOne.App.DirectedGraph.NodeShapes;
using ThreeXPlusOne.App.Enums;
using ThreeXPlusOne.App.Interfaces.DirectedGraph;
using Xunit;

namespace ThreeXPlusOne.UnitTests;

public class ShapeFactoryTests
{
    private readonly IEnumerable<IShape> shapes;

    public ShapeFactoryTests()
    {
        shapes =
            typeof(IShape).Assembly.GetTypes()
                                   .Where(static t => typeof(IShape).IsAssignableFrom(t) && !t.IsInterface)
                                   .Select(Activator.CreateInstance)
                                   .Cast<IShape>();
    }

    /// <summary>
    /// Tests that the CreateShape method returns the exact type specified.
    /// </summary>
    [Fact]
    public void CreateShape_ReturnsSpecifiedShape()
    {
        // Arrange
        var shapeFactory = new ShapeFactory(shapes);

        // Act
        var shape = shapeFactory.CreateShape([ShapeType.SemiCircle]);

        // Assert
        shape.GetType().Should().Be(typeof(SemiCircle));
    }

    /// <summary>
    /// Tests that the CreateShape method returns one of the types specified.
    /// </summary>
    [Fact]
    public void CreateShape_ReturnsOneOfSpecifiedShapes()
    {
        // Arrange
        var shapeFactory = new ShapeFactory(shapes);

        // Act
        var shape = shapeFactory.CreateShape([ShapeType.SemiCircle, ShapeType.Ellipse]);

        // Assert
        shape.GetType().Should().Match(type => type == typeof(SemiCircle) || type == typeof(Ellipse));
    }

    /// <summary>
    /// Tests that the CreateShape method throws an exception for an unsupported shape type.
    /// </summary>
    [Fact]
    public void CreateShape_ThrowsExceptionForUnsupportedShapeType()
    {
        // Arrange
        var shapeFactory = new ShapeFactory(shapes);

        // Act
        var action = () => shapeFactory.CreateShape([(ShapeType)99]);

        // Assert
        action.Should().Throw<ArgumentException>();
    }

    /// <summary>
    /// Tests that the CreateShape method returns a random shape.
    /// </summary>
    [Fact]
    public void CreateShape_ReturnsRandomShape()
    {
        // Arrange
        var shapeFactory = new ShapeFactory(shapes);

        // Act
        var shape = shapeFactory.CreateShape();

        // Assert
        shape.Should().BeAssignableTo<IShape>();
    }
}