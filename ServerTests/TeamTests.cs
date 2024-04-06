using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using NUnit.Framework;
using Web_API_new.Data;
using Web_API_new.Models;

namespace ServerTests
{
    public class TeamTests
    {
        private DataContext _dataContext;

        [SetUp]
        public void Setup()
        {
            // Initialize the in-memory database context
            var options = new DbContextOptionsBuilder<DataContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString()) // Unique database name
                .Options;
            _dataContext = new DataContext(options);

            // Seed the database with test data
            SeedTestData();
        }

        // Seed some test data into the in-memory database
        private void SeedTestData()
        {
            _dataContext.Teams.Add(new TeamModel { Name = "Team 1", LeaderId = "1"});
            _dataContext.Teams.Add(new TeamModel { Name = "Team 2", LeaderId = "2" });
            _dataContext.SaveChanges();
        }

        [Test]
        public async Task Retrieve_All_Teams()
        {
            // Act
            var teams = await _dataContext.Teams.ToListAsync();

            // Assert
            Assert.IsTrue(teams.Count > 0);
        }
        
        [Test]
        public async Task Get_Specific_Team_By_Name()
        {
            // data
            var teamName = "Team 1";
            var searchedTeam = await _dataContext.Teams.FirstOrDefaultAsync(t => t.Name.Equals(teamName));

            // Assert
            Assert.IsTrue(searchedTeam != null);
        }
        
        [Test]
        public async Task Get_Specific_Team_By_Leader()
        {
            // data
            var leaderId = "1";
            var searchedTeam = await _dataContext.Teams.FirstOrDefaultAsync(t => t.LeaderId.Equals(leaderId));

            // Assert
            Assert.IsTrue(searchedTeam != null);
        }
    }
}