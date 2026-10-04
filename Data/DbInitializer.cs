using ArtGalleryFinal.Models;
using BCrypt.Net;
using Microsoft.EntityFrameworkCore;

namespace ArtGalleryFinal.Data
{
    public static class DbInitializer
    {
        // List of actual image files from your wwwroot/images folder
        private static readonly string[] ImageFiles = new[]
        {
            "0412043d-1dad-4351-add5-7ad7ff7aeae1.png",
            "04631ecc-51b1-41d4-801c-324e85aa5456.png",
            "251ba2db-2617-4308-ac56-d96843dc2c10.png",
            "356d3778-d9d9-4dcf-abcc-6f83ff8d3c09.png",
            "3a3047fb-90d6-4f5d-b022-c4ce54720769.png",
            "3afe7754-1d32-4793-b6a0-63db73f5a834.png",
            "3fbcc4e1-96d9-4b08-a506-8b5806029ebb.png",
            "48206bf9-6f81-4a1c-ba41-f7590c6d9e28.png",
            "50a35b25-ad16-4520-b4f4-eb7d19431721.png",
            "55bce8f7-3a40-42cc-869b-1336db5cdae6.png",
            "62ef78bf-ed23-4649-99e4-7c7c63ebf3ae.png",
            "77616e29-6491-4af7-a01c-bccd947e7b21.png",
            "8c2079ca-9154-4624-9649-5a86d314d70d.png",
            "8e5dde68-be16-42cc-b85d-02edcdfca6e4.png",
            "f9a5fa49-08c8-42b3-b3e0-a002cc742284.png"
        };

        public static void Initialize(ArtGalleryContext context)
        {
            context.Database.EnsureCreated();

            // Add the new column/tables if the database already existed (EnsureCreated skips existing DBs)
            context.Database.ExecuteSqlRaw("IF COL_LENGTH('Artwork','Quantity') IS NULL ALTER TABLE Artwork ADD Quantity INT NULL");
            context.Database.ExecuteSqlRaw("IF OBJECT_ID('Wishlists','U') IS NULL CREATE TABLE Wishlists (WishlistId INT IDENTITY PRIMARY KEY, CustomerId INT NOT NULL, ArtId INT NOT NULL)");
            context.Database.ExecuteSqlRaw("IF OBJECT_ID('Complaints','U') IS NULL CREATE TABLE Complaints (ComplaintId INT IDENTITY PRIMARY KEY, CustomerId INT NULL, ArtId INT NULL, Message NVARCHAR(MAX) NOT NULL, CreatedDate DATETIME2 NOT NULL)");

            // ---- 1. Admins (target: 2) ----
            if (context.Admins.Count() < 2)
            {
                var admin1 = new Admin
                {
                    Name = "Admin",
                    Email = "admin@artgallery.com",
                    Password = BCrypt.Net.BCrypt.HashPassword("admin123"),
                    PhoneNo = "1234567890",
                    Country = "Pakistan",
                    Address = "123 Main St, Lahore"
                };
                context.Admins.Add(admin1);

                var admin2 = new Admin
                {
                    Name = "SuperAdmin",
                    Email = "superadmin@artgallery.com",
                    Password = BCrypt.Net.BCrypt.HashPassword("admin456"),
                    PhoneNo = "0987654321",
                    Country = "USA",
                    Address = "456 Pine St, New York"
                };
                context.Admins.Add(admin2);
                context.SaveChanges();
            }

            var adminId = context.Admins.First().AdminId;

            // ---- 2. Categories (added only if missing, so no duplicates) ----
            foreach (var name in new[] { "Painting", "Sculpture", "Digital Art", "Photography", "Mixed Media",
                                         "Watercolor", "Oil Painting", "Printmaking", "Stone Painting" })
            {
                if (!context.ArtCategories.Any(c => c.CategoryName == name))
                    context.ArtCategories.Add(new ArtCategory { CategoryName = name });
            }
            context.SaveChanges();

            // ---- 3. Artists (target: 6) ----
            if (context.Artists.Count() < 6)
            {
                var artists = new[]
                {
                    new Artist
                    {
                        Name = "John Doe",
                        Email = "john@artist.com",
                        Password = BCrypt.Net.BCrypt.HashPassword("artist123"),
                        Country = "USA",
                        Address = "456 Oak St, New York",
                        AdminId = adminId,
                        PhoneNo = "9876543210"
                    },
                    new Artist
                    {
                        Name = "Jane Smith",
                        Email = "jane@artist.com",
                        Password = BCrypt.Net.BCrypt.HashPassword("artist123"),
                        Country = "UK",
                        Address = "789 High St, London",
                        AdminId = adminId,
                        PhoneNo = "0123456789"
                    },
                    new Artist
                    {
                        Name = "Mike Johnson",
                        Email = "mike@artist.com",
                        Password = BCrypt.Net.BCrypt.HashPassword("artist123"),
                        Country = "Canada",
                        Address = "123 Maple Ave, Toronto",
                        AdminId = adminId,
                        PhoneNo = "555-1111"
                    },
                    new Artist
                    {
                        Name = "Emily Brown",
                        Email = "emily@artist.com",
                        Password = BCrypt.Net.BCrypt.HashPassword("artist123"),
                        Country = "Australia",
                        Address = "456 Kangaroo St, Sydney",
                        AdminId = adminId,
                        PhoneNo = "555-2222"
                    },
                    new Artist
                    {
                        Name = "David Wilson",
                        Email = "david@artist.com",
                        Password = BCrypt.Net.BCrypt.HashPassword("artist123"),
                        Country = "Germany",
                        Address = "789 Berliner St, Berlin",
                        AdminId = adminId,
                        PhoneNo = "555-3333"
                    },
                    new Artist
                    {
                        Name = "Sarah Lee",
                        Email = "sarah@artist.com",
                        Password = BCrypt.Net.BCrypt.HashPassword("artist123"),
                        Country = "France",
                        Address = "123 Paris St, Paris",
                        AdminId = adminId,
                        PhoneNo = "555-4444"
                    }
                };
                context.Artists.AddRange(artists);
                context.SaveChanges();
            }
            /*
            // ---- 4. Artworks (target: 12) ----
            var artistList = context.Artists.ToList();
            var categoriesList = context.ArtCategories.ToList();
            if (context.Artworks.Count() < 12 && artistList.Count >= 2)
            {
                var artworks = new List<Artwork>();
                int imageIndex = 0;

                // Helper to get next image filename cyclically
                string GetNextImage() => "/images/" + ImageFiles[imageIndex++ % ImageFiles.Length];

                // Artist 0 (John) – 3 paintings
                artworks.Add(new Artwork
                {
                    ArtName = "Sunset Over Ocean",
                    Title = "Sunset",
                    Description = "A beautiful painting of a sunset over the ocean.",
                    ArtistId = artistList[0].ArtistId,
                    CategoryId = categoriesList.FirstOrDefault(c => c.CategoryName == "Painting")?.CategoryId,
                    Price = 500,
                    ImageUrl = GetNextImage(),
                    Status = "For Sale",
                    AdminApproved = "Yes"
                });
                artworks.Add(new Artwork
                {
                    ArtName = "Mountain Retreat",
                    Title = "Mountains",
                    Description = "Serene mountain landscape with a cabin.",
                    ArtistId = artistList[0].ArtistId,
                    CategoryId = categoriesList.FirstOrDefault(c => c.CategoryName == "Painting")?.CategoryId,
                    Price = 750,
                    ImageUrl = GetNextImage(),
                    Status = "For Sale",
                    AdminApproved = "Pending"
                });
                artworks.Add(new Artwork
                {
                    ArtName = "Abstract Expression",
                    Title = "Abstract",
                    Description = "Vibrant abstract expressionist piece.",
                    ArtistId = artistList[0].ArtistId,
                    CategoryId = categoriesList.FirstOrDefault(c => c.CategoryName == "Painting")?.CategoryId,
                    Price = 400,
                    ImageUrl = GetNextImage(),
                    Status = "For Sale",
                    AdminApproved = "Yes"
                });

                // Artist 1 (Jane) – 3 sculptures/digital
                artworks.Add(new Artwork
                {
                    ArtName = "Abstract Sculpture",
                    Title = "Abstract",
                    Description = "Modern abstract sculpture in bronze.",
                    ArtistId = artistList[1].ArtistId,
                    CategoryId = categoriesList.FirstOrDefault(c => c.CategoryName == "Sculpture")?.CategoryId,
                    Price = 1200,
                    ImageUrl = GetNextImage(),
                    Status = "For Sale",
                    AdminApproved = "Pending"
                });
                artworks.Add(new Artwork
                {
                    ArtName = "Digital Landscape",
                    Title = "Digital",
                    Description = "Digital illustration of a mountain landscape.",
                    ArtistId = artistList[1].ArtistId,
                    CategoryId = categoriesList.FirstOrDefault(c => c.CategoryName == "Digital Art")?.CategoryId,
                    Price = 300,
                    ImageUrl = GetNextImage(),
                    Status = "For Sale",
                    AdminApproved = "Yes"
                });
                artworks.Add(new Artwork
                {
                    ArtName = "Watercolor Flowers",
                    Title = "Flowers",
                    Description = "Delicate watercolor painting of wildflowers.",
                    ArtistId = artistList[1].ArtistId,
                    CategoryId = categoriesList.FirstOrDefault(c => c.CategoryName == "Watercolor")?.CategoryId,
                    Price = 250,
                    ImageUrl = GetNextImage(),
                    Status = "For Sale",
                    AdminApproved = "Yes"
                });

                // Artist 2 (Mike) – 2 mixed media + 1 photography
                artworks.Add(new Artwork
                {
                    ArtName = "Urban Collage",
                    Title = "Collage",
                    Description = "Mixed media collage of urban scenes.",
                    ArtistId = artistList[2].ArtistId,
                    CategoryId = categoriesList.FirstOrDefault(c => c.CategoryName == "Mixed Media")?.CategoryId,
                    Price = 600,
                    ImageUrl = GetNextImage(),
                    Status = "For Sale",
                    AdminApproved = "Pending"
                });
                artworks.Add(new Artwork
                {
                    ArtName = "Cityscape Photography",
                    Title = "Cityscape",
                    Description = "Stunning black and white photograph of city skyline.",
                    ArtistId = artistList[2].ArtistId,
                    CategoryId = categoriesList.FirstOrDefault(c => c.CategoryName == "Photography")?.CategoryId,
                    Price = 450,
                    ImageUrl = GetNextImage(),
                    Status = "For Sale",
                    AdminApproved = "Yes"
                });

                // Artist 3 (Emily) – 2 oil paintings
                artworks.Add(new Artwork
                {
                    ArtName = "Portrait of a Lady",
                    Title = "Portrait",
                    Description = "Classic oil portrait with rich tones.",
                    ArtistId = artistList[3].ArtistId,
                    CategoryId = categoriesList.FirstOrDefault(c => c.CategoryName == "Oil Painting")?.CategoryId,
                    Price = 900,
                    ImageUrl = GetNextImage(),
                    Status = "For Sale",
                    AdminApproved = "Yes"
                });
                artworks.Add(new Artwork
                {
                    ArtName = "Still Life with Fruit",
                    Title = "Still Life",
                    Description = "Oil painting of a bowl of fruit.",
                    ArtistId = artistList[3].ArtistId,
                    CategoryId = categoriesList.FirstOrDefault(c => c.CategoryName == "Oil Painting")?.CategoryId,
                    Price = 350,
                    ImageUrl = GetNextImage(),
                    Status = "For Sale",
                    AdminApproved = "Pending"
                });

                // Artist 4 (David) – 2 printmaking
                artworks.Add(new Artwork
                {
                    ArtName = "Linocut Forest",
                    Title = "Forest",
                    Description = "Handmade linocut print of a forest scene.",
                    ArtistId = artistList[4].ArtistId,
                    CategoryId = categoriesList.FirstOrDefault(c => c.CategoryName == "Printmaking")?.CategoryId,
                    Price = 200,
                    ImageUrl = GetNextImage(),
                    Status = "For Sale",
                    AdminApproved = "Yes"
                });
                artworks.Add(new Artwork
                {
                    ArtName = "Etching of City",
                    Title = "City Etching",
                    Description = "Detailed etching of a city alleyway.",
                    ArtistId = artistList[4].ArtistId,
                    CategoryId = categoriesList.FirstOrDefault(c => c.CategoryName == "Printmaking")?.CategoryId,
                    Price = 280,
                    ImageUrl = GetNextImage(),
                    Status = "For Sale",
                    AdminApproved = "Pending"
                });

                // Artist 5 (Sarah) – 1 more painting (to reach 12)
                artworks.Add(new Artwork
                {
                    ArtName = "Evening Glow",
                    Title = "Evening",
                    Description = "Painting of a peaceful evening landscape.",
                    ArtistId = artistList[5].ArtistId,
                    CategoryId = categoriesList.FirstOrDefault(c => c.CategoryName == "Painting")?.CategoryId,
                    Price = 650,
                    ImageUrl = GetNextImage(),
                    Status = "For Sale",
                    AdminApproved = "Yes"
                });

                context.Artworks.AddRange(artworks);
                context.SaveChanges();
            }
            */
            // ---- 5. Customers (target: 6) ----
            if (context.Customers.Count() < 6)
            {
                var customers = new[]
                {
                    new Customer
                    {
                        Name = "Alice Buyer",
                        Email = "alice@buyer.com",
                        Password = BCrypt.Net.BCrypt.HashPassword("customer123"),
                        Country = "Canada",
                        Address = "123 Maple St, Toronto",
                        AdminId = adminId,
                        PhoneNo = "555-1234"
                    },
                    new Customer
                    {
                        Name = "Bob Collector",
                        Email = "bob@collector.com",
                        Password = BCrypt.Net.BCrypt.HashPassword("customer123"),
                        Country = "Australia",
                        Address = "456 Kangaroo St, Sydney",
                        AdminId = adminId,
                        PhoneNo = "555-5678"
                    },
                    new Customer
                    {
                        Name = "Charlie ArtLover",
                        Email = "charlie@buyer.com",
                        Password = BCrypt.Net.BCrypt.HashPassword("customer123"),
                        Country = "UK",
                        Address = "789 London St, London",
                        AdminId = adminId,
                        PhoneNo = "555-9999"
                    },
                    new Customer
                    {
                        Name = "Diana Dealer",
                        Email = "diana@buyer.com",
                        Password = BCrypt.Net.BCrypt.HashPassword("customer123"),
                        Country = "USA",
                        Address = "321 Broadway, New York",
                        AdminId = adminId,
                        PhoneNo = "555-0000"
                    },
                    new Customer
                    {
                        Name = "Eve Curator",
                        Email = "eve@buyer.com",
                        Password = BCrypt.Net.BCrypt.HashPassword("customer123"),
                        Country = "Germany",
                        Address = "456 Berlin St, Berlin",
                        AdminId = adminId,
                        PhoneNo = "555-1111"
                    },
                    new Customer
                    {
                        Name = "Frank Framer",
                        Email = "frank@buyer.com",
                        Password = BCrypt.Net.BCrypt.HashPassword("customer123"),
                        Country = "France",
                        Address = "789 Paris St, Paris",
                        AdminId = adminId,
                        PhoneNo = "555-2222"
                    }
                };
                context.Customers.AddRange(customers);
                context.SaveChanges();
            }

            // ---- 6. Orders, OrderDetails, Payments, Shipments (more orders) ----
            var customerList = context.Customers.ToList();
            var artworkList = context.Artworks.ToList();
            if (customerList.Count >= 2 && artworkList.Count >= 4 && context.Orders.Count() < 8)
            {
                // Helper method to create orders
                void CreateOrder(Customer customer, List<Artwork> arts, string status, string paymentMethod, bool paid)
                {
                    int totalAmount = arts.Sum(a => a.Price ?? 0);
                    var order = new Order
                    {
                        CustomerId = customer.CustomerId,
                        OrderDate = DateOnly.FromDateTime(DateTime.Now.AddDays(-new Random().Next(1, 30))),
                        OrderAmount = totalAmount,
                        Discount = 0,
                        OrderStatus = status,
                        AdminId = adminId
                    };
                    context.Orders.Add(order);
                    context.SaveChanges();

                    foreach (var art in arts)
                    {
                        context.OrderDetails.Add(new OrderDetail
                        {
                            OrderId = order.OrderId,
                            ArtId = art.ArtId,
                            Quantity = 1,
                            Price = art.Price ?? 0
                        });
                    }
                    context.SaveChanges();

                    int shipmentCharges = 250;
                    context.Payments.Add(new Payment
                    {
                        OrderId = order.OrderId,
                        PaymentDate = DateOnly.FromDateTime(DateTime.Now),
                        Amount = totalAmount + shipmentCharges,
                        PaymentStatus = paid ? "Paid" : "Pending",
                        PaymentMethod = paymentMethod,
                        AdminId = adminId
                    });
                    context.SaveChanges();

                    context.Shipments.Add(new Shipment
                    {
                        OrderId = order.OrderId,
                        ShipmentCharges = shipmentCharges,
                        Address = customer.Address,
                        Country = customer.Country,
                        Email = customer.Email,
                        PhoneNo = customer.PhoneNo,
                        ShipmentDate = DateOnly.FromDateTime(DateTime.Now),
                        AdminId = adminId
                    });
                    context.SaveChanges();
                }

                // Order 1: Alice - 2 artworks (delivered, COD)
                CreateOrder(customerList[0], artworkList.Take(2).ToList(), "Delivered", "CashOnDelivery", true);

                // Order 2: Alice - 1 artwork (pending, Stripe)
                CreateOrder(customerList[0], artworkList.Skip(2).Take(1).ToList(), "Pending", "Stripe", false);

                // Order 3: Bob - 3 artworks (shipped, JazzCash)
                CreateOrder(customerList[1], artworkList.Skip(3).Take(3).ToList(), "Shipped", "JazzCash", true);

                // Order 4: Charlie - 2 artworks (pending, COD)
                CreateOrder(customerList[2], artworkList.Skip(6).Take(2).ToList(), "Pending", "CashOnDelivery", false);

                // Order 5: Diana - 1 artwork (delivered, Stripe)
                CreateOrder(customerList[3], artworkList.Skip(8).Take(1).ToList(), "Delivered", "Stripe", true);

                // Order 6: Eve - 2 artworks (shipped, JazzCash)
                CreateOrder(customerList[4], artworkList.Skip(9).Take(2).ToList(), "Shipped", "JazzCash", true);

                // Order 7: Frank - 1 artwork (pending, COD)
                CreateOrder(customerList[5], artworkList.Skip(11).Take(1).ToList(), "Pending", "CashOnDelivery", false);

                // Order 8: Alice - 1 more artwork (delivered, COD) – to have 3 orders for Alice
                CreateOrder(customerList[0], artworkList.Skip(1).Take(1).ToList(), "Delivered", "CashOnDelivery", true);
            }

            // ---- 6b. Ensure sales data spans at least 5 months (for Sales Dashboard graph) ----
            var ordersWithDate = context.Orders.Where(o => o.OrderDate != null).ToList();
            var distinctMonthCount = ordersWithDate
                .Select(o => new { o.OrderDate.Value.Year, o.OrderDate.Value.Month })
                .Distinct()
                .Count();

            if (distinctMonthCount < 5 && customerList.Any())
            {
                var sampleCustomerId = customerList.First().CustomerId;
                var monthsAgoOffsets = new[] { 150, 120, 90, 60, 30 };

                foreach (var daysAgo in monthsAgoOffsets)
                {
                    var orderDate = DateOnly.FromDateTime(DateTime.Now.AddDays(-daysAgo));
                    bool monthAlreadyHasOrder = ordersWithDate.Any(o =>
                        o.OrderDate.Value.Year == orderDate.Year &&
                        o.OrderDate.Value.Month == orderDate.Month);

                    if (!monthAlreadyHasOrder)
                    {
                        context.Orders.Add(new Order
                        {
                            CustomerId = sampleCustomerId,
                            OrderDate = orderDate,
                            OrderAmount = 500 + daysAgo * 50,
                            Discount = 0,
                            OrderStatus = "Delivered",
                            AdminId = adminId
                        });
                    }
                }
                context.SaveChanges();
            }
            // ---- 7. Ratings (more reviews for delivered orders) ----
            var deliveredOrders = context.Orders.Where(o => o.OrderStatus == "Delivered").ToList();
            if (deliveredOrders.Any() && context.Ratings.Count() < deliveredOrders.Count)
            {
                foreach (var order in deliveredOrders)
                {
                    if (!context.Ratings.Any(r => r.OrderId == order.OrderId))
                    {
                        var customer = context.Customers.FirstOrDefault(c => c.CustomerId == order.CustomerId);
                        var orderDetails = context.OrderDetails.Where(od => od.OrderId == order.OrderId).ToList();
                        if (customer != null && orderDetails.Any())
                        {
                            var artId = orderDetails.First().ArtId;
                            var rating = new Rating
                            {
                                ArtId = artId,
                                OrderId = order.OrderId,
                                CustomerId = customer.CustomerId,
                                RatingValue = new Random().Next(3, 6),
                                Review = $"Great artwork! {new[] { "Loved it", "Beautiful", "Stunning", "Amazing", "Excellent" }[new Random().Next(0, 5)]}",
                                ReviewDate = DateTime.Now.AddDays(-new Random().Next(1, 10))
                            };
                            context.Ratings.Add(rating);
                        }
                    }
                }
                context.SaveChanges();
            }

            // Artworks that have no quantity yet get a default of 3 (artist can change it)
            context.Database.ExecuteSqlRaw("UPDATE Artwork SET Quantity = 3 WHERE Quantity IS NULL");
        }
    }
}