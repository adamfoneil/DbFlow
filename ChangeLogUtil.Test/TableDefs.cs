﻿using Microsoft.VisualStudio.TestTools.UnitTesting;
using SqlServer.LocalDb;
using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;

namespace ChangeLogUtil.Test
{
    [TestClass]
    public class TableDefs
    {
        [TestMethod]
        public void Appointment()
        {
            var xml = GetResource("Resources.Appointment.xml");
            var service = new TableDefXmlRenderer();
        }

        private string GetResource(string name)
        {
            using (var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(name))
            {
                return new StreamReader(stream).ReadToEnd();
            }
        }
    }
}
