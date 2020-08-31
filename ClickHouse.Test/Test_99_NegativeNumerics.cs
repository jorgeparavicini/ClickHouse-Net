using System;
using System.Collections.Generic;
using System.Data;
using System.Threading;
using ClickHouse.Ado;
using NUnit.Framework;

namespace ClickHouse.Test {
    [TestFixture]
    public class Test_99_NegativeNumerics {
        [OneTimeSetUp]
        public void CreateStructures() {
            using (var cnn = ConnectionHandler.GetConnection()) {
                cnn.CreateCommand("DROP TABLE IF EXISTS test_99_neg_num").ExecuteNonQuery();
                cnn.CreateCommand("CREATE TABLE test_99_neg_num (FixedDate Date, Amount1 Decimal(16,4), Amount2 Decimal(20,8), Amount3 Decimal(22,10) ) ENGINE = MergeTree PARTITION BY toYYYYMM(FixedDate) ORDER BY FixedDate SETTINGS index_granularity = 8192").ExecuteNonQuery();
            }

            Thread.Sleep(1000);
        }

        [Test]
        public void TestRoundtrip() {
            using (var cnn = ConnectionHandler.GetConnection()) {
                cnn.CreateCommand("INSERT INTO test_99_neg_num (FixedDate,Amount1,Amount2,Amount3) VALUES @bulk").AddParameter("bulk", DbType.Object, new object[] {
                       new object[] {DateTime.Now, -100m,-100m,-100m}
                   })
                   .ExecuteNonQuery();
                var values = new List<Tuple<DateTime, decimal,decimal,decimal>>();
                using (var cmd = cnn.CreateCommand("SELECT FixedDate,Amount1,Amount2,Amount3 FROM test_99_neg_num ORDER BY FixedDate"))
                using (var reader = cmd.ExecuteReader()) {
                    reader.ReadAll(r => { values.Add(Tuple.Create(r.GetDateTime(0), r.GetDecimal(1),r.GetDecimal(2),r.GetDecimal(3))); });
                }

                Assert.AreEqual(-100, (double) values[0].Item2, .33);
                Assert.AreEqual(-100, (double) values[0].Item3, .33);
                Assert.AreEqual(-100, (double) values[0].Item4, .33);
            }
        }
    }
}
