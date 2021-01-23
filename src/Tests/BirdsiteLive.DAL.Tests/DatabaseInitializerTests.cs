using System;
using System.Linq;
using System.Threading.Tasks;
using BirdsiteLive.DAL.Contracts;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace BirdsiteLive.DAL.Tests
{
    [TestClass]
    public class DatabaseInitializerTests
    {
        [TestMethod]
        public async Task DbInitAsync_UpToDate_Test()
        {
            #region Stubs
            var current = new Version(2, 3);
            var mandatory = new Version(2, 3);
            #endregion

            #region Mocks
            var dbInitializerDal = new Mock<IDbInitializerDal>(MockBehavior.Strict);

            dbInitializerDal
                .Setup(x => x.GetCurrentDbVersionAsync())
                .ReturnsAsync(current);

            dbInitializerDal
                .Setup(x => x.GetMandatoryDbVersion())
                .Returns(mandatory);
            #endregion

            var dbInitializer = new DatabaseInitializer(dbInitializerDal.Object);
            await dbInitializer.InitAndMigrateDbAsync();

            #region Validations
            dbInitializerDal.VerifyAll();
            #endregion
        }

        [TestMethod]
        public async Task DbInitAsync_NoDb_Test()
        {
            #region Stubs
            var current = (Version)null;
            var mandatory = new Version(1, 0);

            var migrationPatterns = new Tuple<Version, Version>[0];
            #endregion

            #region Mocks
            var dbInitializerDal = new Mock<IDbInitializerDal>(MockBehavior.Strict);

            dbInitializerDal
                .Setup(x => x.GetCurrentDbVersionAsync())
                .ReturnsAsync(current);

            dbInitializerDal
                .Setup(x => x.GetMandatoryDbVersion())
                .Returns(mandatory);

            dbInitializerDal
                .Setup(x => x.InitDbAsync())
                .ReturnsAsync(new Version(1, 0));

            dbInitializerDal
                .Setup(x => x.GetMigrationPatterns())
                .Returns(migrationPatterns);
            #endregion

            var dbInitializer = new DatabaseInitializer(dbInitializerDal.Object);
            await dbInitializer.InitAndMigrateDbAsync();

            #region Validations
            dbInitializerDal.VerifyAll();
            #endregion
        }

        [TestMethod]
        public async Task DbInitAsync_NoDb_Migration_Test()
        {
            #region Stubs
            var current = (Version)null;
            var mandatory = new Version(2, 3);

            var migrationPatterns = new Tuple<Version, Version>[]
            {
                new Tuple<Version, Version>(new Version(1,0), new Version(1,7)),
                new Tuple<Version, Version>(new Version(1,7), new Version(2,0)),
                new Tuple<Version, Version>(new Version(2,0), new Version(2,3))
            };
            #endregion

            #region Mocks
            var dbInitializerDal = new Mock<IDbInitializerDal>(MockBehavior.Strict);

            dbInitializerDal
                .Setup(x => x.GetCurrentDbVersionAsync())
                .ReturnsAsync(current);

            dbInitializerDal
                .Setup(x => x.GetMandatoryDbVersion())
                .Returns(mandatory);

            dbInitializerDal
                .Setup(x => x.InitDbAsync())
                .ReturnsAsync(new Version(1, 0));

            dbInitializerDal
                .Setup(x => x.GetMigrationPatterns())
                .Returns(migrationPatterns);

            foreach (var m in migrationPatterns)
            {
                dbInitializerDal
                    .Setup(x => x.MigrateDbAsync(
                        It.Is<Version>(y => y == m.Item1),
                        It.Is<Version>(y => y == m.Item2)
                        ))
                    .ReturnsAsync(m.Item2);
            }
            #endregion

            var dbInitializer = new DatabaseInitializer(dbInitializerDal.Object);
            await dbInitializer.InitAndMigrateDbAsync();

            #region Validations
            dbInitializerDal.VerifyAll();
            #endregion
        }

        [TestMethod]
        public async Task DbInitAsync_HasDb_Migration_Test()
        {
            #region Stubs
            var current = new Version(1, 7);
            var mandatory = new Version(2, 3);

            var migrationPatterns = new Tuple<Version, Version>[]
            {
                new Tuple<Version, Version>(new Version(1,0), new Version(1,7)),
                new Tuple<Version, Version>(new Version(1,7), new Version(2,0)),
                new Tuple<Version, Version>(new Version(2,0), new Version(2,3))
            };
            #endregion

            #region Mocks
            var dbInitializerDal = new Mock<IDbInitializerDal>(MockBehavior.Strict);

            dbInitializerDal
                .Setup(x => x.GetCurrentDbVersionAsync())
                .ReturnsAsync(current);

            dbInitializerDal
                .Setup(x => x.GetMandatoryDbVersion())
                .Returns(mandatory);

            dbInitializerDal
                .Setup(x => x.GetMigrationPatterns())
                .Returns(migrationPatterns);

            foreach (var m in migrationPatterns.Skip(1))
            {
                dbInitializerDal
                    .Setup(x => x.MigrateDbAsync(
                        It.Is<Version>(y => y == m.Item1),
                        It.Is<Version>(y => y == m.Item2)
                    ))
                    .ReturnsAsync(m.Item2);
            }
            #endregion

            var dbInitializer = new DatabaseInitializer(dbInitializerDal.Object);
            await dbInitializer.InitAndMigrateDbAsync();

            #region Validations
            dbInitializerDal.VerifyAll();
            #endregion
        }

        [TestMethod]
        [ExpectedException(typeof(Exception))]
        public async Task DbInitAsync_NoDb_Migration_Error_Test()
        {
            #region Stubs
            var current = (Version)null;
            var mandatory = new Version(2, 3);

            var migrationPatterns = new Tuple<Version, Version>[]
            {
                new Tuple<Version, Version>(new Version(1,0), new Version(1,7)),
                new Tuple<Version, Version>(new Version(1,7), new Version(2,0)),
                new Tuple<Version, Version>(new Version(2,0), new Version(2,2))
            };
            #endregion

            #region Mocks
            var dbInitializerDal = new Mock<IDbInitializerDal>(MockBehavior.Strict);

            dbInitializerDal
                .Setup(x => x.GetCurrentDbVersionAsync())
                .ReturnsAsync(current);

            dbInitializerDal
                .Setup(x => x.GetMandatoryDbVersion())
                .Returns(mandatory);

            dbInitializerDal
                .Setup(x => x.InitDbAsync())
                .ReturnsAsync(new Version(1, 0));

            dbInitializerDal
                .Setup(x => x.GetMigrationPatterns())
                .Returns(migrationPatterns);

            foreach (var m in migrationPatterns)
            {
                dbInitializerDal
                    .Setup(x => x.MigrateDbAsync(
                        It.Is<Version>(y => y == m.Item1),
                        It.Is<Version>(y => y == m.Item2)
                    ))
                    .ReturnsAsync(m.Item2);
            }
            #endregion

            var dbInitializer = new DatabaseInitializer(dbInitializerDal.Object);
            try
            {
                await dbInitializer.InitAndMigrateDbAsync();
            }
            finally
            {
                #region Validations
                dbInitializerDal.VerifyAll();
                #endregion
            }
        }
    }
}
