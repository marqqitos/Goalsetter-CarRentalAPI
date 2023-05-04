using CarRental.Entities;

namespace CarRental.DAL
{
    public static class RentalInitializer
    {
        public static void Seed(RentalContext context)
        {
            context.Database.EnsureCreated();

            // Look for any vehicles.
            if (context.Vehicles.Any() || context.Clients.Any() || context.Rentals.Any())
            {
                return; // Database has already been seeded.
            }

            // Create some vehicles
            var vehicles = new List<Vehicle>
            {
                new Vehicle { ChassisNumber = "VIN123", Make = "Toyota", Model = "Corolla", PricePerDay = 40, IsActive = true },
                new Vehicle { ChassisNumber = "VIN456", Make = "Honda", Model = "Accord", PricePerDay = 50, IsActive = true },
                new Vehicle { ChassisNumber = "VIN789", Make = "Ford", Model = "Mustang", PricePerDay = 80, IsActive = true },
                new Vehicle { ChassisNumber = "VINABC", Make = "Chevrolet", Model = "Camaro", PricePerDay = 75, IsActive = true },
                new Vehicle { ChassisNumber = "VINDEF", Make = "Tesla", Model = "Model 3", PricePerDay = 100, IsActive = true }
            };
            vehicles.ForEach(v => context.Vehicles.Add(v));

            // Create some clients
            var clients = new List<Client>
            {
                new Client { FirstName = "John", LastName = "Doe", Email = "johndoe@example.com", IsActive = true },
                new Client { FirstName = "Jane", LastName = "Smith", Email = "janesmith@example.com", IsActive = true },
                new Client { FirstName = "Bob", LastName = "Jones", Email = "bobjones@example.com", IsActive = true },
                new Client { FirstName = "Alice", LastName = "Lee", Email = "alicelee@example.com", IsActive = true },
                new Client { FirstName = "David", LastName = "Kim", Email = "davidkim@example.com", IsActive = true }
            };
            clients.ForEach(c => context.Clients.Add(c));

            // Create some rentals
            var rentals = new List<Rental>
            {
                new Rental { Vehicle = vehicles[0], Client = clients[0], StartDate = DateTime.Today.AddDays(1), EndDate = DateTime.Today.AddDays(7), RentalChargePrice = 280, IsActive = true },
                new Rental { Vehicle = vehicles[1], Client = clients[1], StartDate = DateTime.Today.AddDays(3), EndDate = DateTime.Today.AddDays(10), RentalChargePrice = 400, IsActive = true },
                new Rental { Vehicle = vehicles[2], Client = clients[2], StartDate = DateTime.Today.AddDays(5), EndDate = DateTime.Today.AddDays(12), RentalChargePrice = 640, IsActive = true },
                new Rental { Vehicle = vehicles[3], Client = clients[3], StartDate = DateTime.Today.AddDays(7), EndDate = DateTime.Today.AddDays(14), RentalChargePrice = 600, IsActive = true },
                new Rental { Vehicle = vehicles[4], Client = clients[4], StartDate = DateTime.Today.AddDays(9), EndDate = DateTime.Today.AddDays(16), RentalChargePrice = 800, IsActive = true }
            };
            rentals.ForEach(r => context.Rentals.Add(r));

            // Save changes to the database
            context.SaveChanges();
        }
    }

}
