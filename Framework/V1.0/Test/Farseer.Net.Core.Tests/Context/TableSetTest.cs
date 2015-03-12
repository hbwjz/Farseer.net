﻿using System;
using Demo.PO.Table.Members;
using FS.Core.Client.SqlServer;
using FS.Core.Context;
using FS.Core.Data;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Farseer.Net.Core.Tests.Context
{
    [TestClass]
    public class TableSetTest
    {
        [TestMethod]
        public void TestMethod1()
        {
            for (var i = 0; i < 1000; i++)
            {
                TableContext<UserPO>.Data.Insert(new UserPO() { UserName = "xx" });
            }
        }

        [TestMethod]
        public void TestMethod2()
        {
            using (var table = new TableContext<UserPO>())
            {
                for (var i = 0; i < 1000; i++)
                {
                    table.TableSet.Insert(new UserPO() { UserName = "yy" });
                }
                table.SaveChanges();
            }
        }
    }
}