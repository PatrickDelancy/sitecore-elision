using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Xml;
using Sitecore.Mvc.Helpers;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Elision.Tests.Configuration
{
    [TestClass]
    public class ConfigReferencedTypes
    {
        [TestMethod]
        public void AllReferencedTypesExist()
        {
            //get reference to all config files in the main web project
            //find all nodes that reference a type
            //try to retrieve the type definition to make sure it exists


            var issues = new Dictionary<string, List<string>>();

            var files = GetAllConfigFiles();
            foreach (var file in files)
            {
                issues.Add(file, new List<string>());

                var nodesWithTypes = GetNodesWithTypeRefs(file);
                if (nodesWithTypes == null || nodesWithTypes.Count == 0)
                {
                    Console.WriteLine("WARNING: No type references found in {0}", file);
                    continue;
                }

                foreach (var node in nodesWithTypes.Cast<XmlNode>())
                {
                    var typeName = GetTypeRefFromNode(node);
                    try
                    {
                        var typeInfo = TypeHelper.GetType(typeName);
                        if (typeInfo == null)
                            issues[file].Add(node.OuterXml);
                        else if (System.Diagnostics.Debugger.IsAttached)
                            Console.WriteLine("INFO: Type resolved \"{0}\"", typeInfo.AssemblyQualifiedName);
                    }
                    catch (Exception ex)
                    {
                        issues[file].Add(node.OuterXml + "\r\nERROR: " + ex.Message);
                    }
                }
            }

            if (issues.Any(x => x.Value.Any()))
                Assert.Fail("Failed to find the type definitions for these types. Make sure the test project references (CopyLocal=True) all necessary Sitecore Dlls that contain all base types used.\r\n\r\n"
                            + string.Join("\r\n\r\n", issues.Where(x => x.Value.Any()).Select(x => x.Key + "\r\n\r\n" + string.Join("\r\n", x.Value))));
        }

        [TestMethod]
        public void AllReferencedTypesWithoutDefaultCtorUseFactory()
        {
            var issues = new Dictionary<string, List<string>>();

            var files = GetAllConfigFiles();
            foreach (var file in files)
            {
                issues.Add(file, new List<string>());

                var nodesWithTypes = GetNodesWithTypeRefs(file);
                if (nodesWithTypes == null || nodesWithTypes.Count == 0)
                {
                    Console.WriteLine("WARNING: No type references found in {0}", file);
                    continue;
                }

                foreach (var node in nodesWithTypes.Cast<XmlNode>().Where(x => x.Name == "processor"))
                {
                    if (node.SelectNodes("./param").Count > 0)
                        continue;

                    var typeName = GetTypeRefFromNode(node);
                    try
                    {
                        var typeInfo = Type.GetType(typeName);
                        if (typeInfo == null)
                        {
                            issues[file].Add(node.OuterXml);
                        }
                        else
                        {
                            if (Debugger.IsAttached)
                                Console.WriteLine("INFO: Type resolved \"{0}\"", typeInfo.AssemblyQualifiedName);

                            var ctors = typeInfo.GetConstructors(BindingFlags.Instance | BindingFlags.Public);
                            if (ctors.All(x => x.GetParameters().Any()) && (node.Attributes["factory"] == null || node.Attributes["factory"].Value != "ElisionObjectFactory"))
                                issues[file].Add(node.OuterXml + "\r\nERROR: Referenced type does not have a default constructor, so it must be initialized by a factory, and it is not.");
                        }
                    }
                    catch (Exception ex)
                    {
                        issues[file].Add(node.OuterXml + "\r\nERROR: " + ex.Message);
                    }
                }
            }

            if (issues.Any(x => x.Value.Any()))
                Assert.Fail("Failed to find the type definitions for these types. Make sure the test project references (CopyLocal=True) all necessary Sitecore Dlls that contain all base types used.\r\n\r\n"
                            + string.Join("\r\n\r\n", issues.Where(x => x.Value.Any()).Select(x => x.Key + "\r\n\r\n" + string.Join("\r\n", x.Value))));
        }

        private static string GetTypeRefFromNode(XmlNode node)
        {
            var typeName = node.Attributes["type"] == null ? "" : node.Attributes["type"].Value;
            if (!TypeHelper.LooksLikeTypeName(typeName))
                typeName = node.Attributes["ref"] == null ? "" : node.Attributes["ref"].Value;
            return typeName;
        }

        private static XmlNodeList GetNodesWithTypeRefs(string file)
        {
            var doc = new XmlDocument();
            doc.Load(file);

            var nodesWithTypes = doc.SelectNodes("//*[starts-with(@type,'Elision') or starts-with(@ref,'Elision')]");
            return nodesWithTypes;
        }

        private static IEnumerable<string> GetAllConfigFiles()
        {
            var files = Directory.GetFiles(
                @"..\..\src\Elision.Web\App_Config\",
                "*.config",
                SearchOption.AllDirectories
                );

            if (files.Length == 0)
                Assert.Inconclusive("No configuration files were found. Please confirm that this is correct.");
            return files;
        }
    }
}
