using ReservationManagementApi_06.Domain;
using ReservationManagementApi_06.Exceptions;
using System;
using System.Collections.Generic;
using System.Text;

namespace ReservationManagementApi_06.Tests.Domain
{
    public class ResourceTests
    {
        private static Resource CreateDefaultResource() 
        {
            return new Resource(
            "Name",
            "Descripton",
            12,
            99.9m
            );
        }
        [Fact]
        public void Constructor_ShouldSetIsActiveToTrue_WhenResourceIsCreated()
        {
            // Arrange
            var name = "name";
            var description = "description";
            var capacity = 10;
            var hourlyRate = 60.33m;
            // Act
            var result = new Resource(name, description, capacity, hourlyRate);
            // Assert
            Assert.True(result.IsActive);
        }
        [Fact]
        public void Constructor_ShouldThrowDomainException_WhenNameIsEmpty()
        {
            // Arrange
            var name = "";
            var description = "description";
            var capacity = 10;
            var hourlyRate = 60.33m;
            // Act and assert
            Assert.Throws<DomainException>(() => new Resource(name, description, capacity, hourlyRate));
        }
        [Theory]
        [InlineData(0)]
        [InlineData(-1)]
        public void Constructor_ShouldThrowDomainException_WhenCapacityIsZeroOrNegative(int capacity)
        {
            // Arrange
            var name = "name";
            var description = "description";
            var hourlyRate = 60.33m;
            // Act and assert
            Assert.Throws<DomainException>(() => new Resource(name, description, capacity, hourlyRate));
        }
        [Theory]
        [InlineData(0)]
        [InlineData(-1)]
        public void Constructor_ShouldThrowDomainException_WhenHourlyRateIsZeroOrNegative(double hourlyRateInput)
        {
            // Arrange
            var name = "name";
            var description = "description";
            var capacity = 10;
            var hourlyRate = (decimal)hourlyRateInput;
            // Act and assert
            Assert.Throws<DomainException>(() => new Resource(name, description, capacity, hourlyRate));
        }
        [Fact]
        public void Deactivate_ShouldSetIsActiveToFalse_WhenResourceIsActive()
        {
            // Arrange
            var resource = CreateDefaultResource();
            // Act
            resource.Deactivate();
            // Assert
            Assert.False(resource.IsActive);
        }
        [Fact]
        public void Activate_ShouldSetIsActiveToTrue_WhenResourceIsInactive()
        {
            // Arrange
            var resource = CreateDefaultResource();
            resource.Deactivate();
            // Act
            resource.Activate();
            // Assert
            Assert.True(resource.IsActive);
        }
        [Fact]
        public void Update_ShouldChangeResourceData_WhenInputIsValid()
        {
            // Arrange
            var resource = CreateDefaultResource();
            var newName = "new name";
            var newDecription = "new Descriptcion";
            var newCapacity = 1111;
            var newHourlyRate = 123.34m;
            // Act
            resource.Update(newName, newDecription,newCapacity,newHourlyRate);
            // Assert
            Assert.Equal(resource.Name, newName);
            Assert.Equal(resource.Description, newDecription);
            Assert.Equal(resource.Capacity, newCapacity);
            Assert.Equal(resource.HourlyRate, newHourlyRate);
        }
    }
}
