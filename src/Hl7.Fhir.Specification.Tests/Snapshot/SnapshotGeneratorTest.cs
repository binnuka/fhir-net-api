﻿/* 
 * Copyright (c) 2016, Furore (info@furore.com) and contributors
 * See the file CONTRIBUTORS for details.
 * 
 * This file is licensed under the BSD 3-Clause license
 * available at https://raw.githubusercontent.com/ewoutkramer/fhir-net-api/master/LICENSE
 */

using System;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Hl7.Fhir.Model;
using Hl7.Fhir.Support;
using System.Diagnostics;
using System.IO;
using Hl7.Fhir.Serialization;
using Hl7.Fhir.Introspection;
using System.Collections.Generic;
using Hl7.Fhir.Specification.Source;
using Hl7.Fhir.Specification.Snapshot;
using Hl7.Fhir.Specification.Navigation;
using Hl7.Fhir.Rest;
using System.Text;
using System.Xml;
using Hl7.Fhir.Utility;

namespace Hl7.Fhir.Specification.Tests
{
    [TestClass]
#if PORTABLE45
	public class PortableSnapshotGeneratorTest
#else
    public class SnapshotGeneratorTest2
#endif
    {
        SnapshotGenerator _generator;
        IResourceResolver _testResolver;
        TimingSource _source;

        readonly SnapshotGeneratorSettings _settings = new SnapshotGeneratorSettings()
        {
            // Throw on unresolved profile references; must include in TestData folder
            GenerateSnapshotForExternalProfiles = true,
            ForceRegenerateSnapshots = true,
            GenerateExtensionsOnConstraints = false,
            GenerateAnnotationsOnConstraints = false,
            GenerateElementIds = false // STU3
        };

        [TestInitialize]
        public void Setup()
        {
            Hl7.Fhir.FhirPath.PocoNavigatorExtensions.PrepareFhirSybolTableFunctions();

            var dirSource = new DirectorySource("TestData/snapshot-test", includeSubdirectories: true);
            _source = new TimingSource(dirSource);
            _testResolver = new CachedResolver(
                new MultiResolver(
                    _source,
                    new ZipSource("specification.zip")));
        }

        // [WMR 20160718] Generate snapshot for extension definition fails with exception:
        // System.ArgumentException: structure is not a constraint or extension

#if false
        [TestMethod]
        public void FindDerivedExtensions()
        {
            var sdUris = _source.ListResourceUris(ResourceType.StructureDefinition);
            foreach (var uri in sdUris)
            {
                var sd = _source.FindStructureDefinition(uri);
                if (sd.ConstrainedType == FHIRAllTypes.Extension && sd.Base != "http://hl7.org/fhir/StructureDefinition/Extension")
                {
                    var origin = sd.Annotation<OriginInformation>();
                    Debug.Print($"Derived extension: uri = '{uri}' origin = '{origin?.Origin}'");
                }
            }

            // var sdInfo = testSD.Annotation<OriginInformation>();
        }
#endif

        [TestMethod]
        public void GenerateExtensionSnapshot()
        {
            // var sd = _testResolver.FindStructureDefinition(@"http://example.org/fhir/StructureDefinition/string-translation");
            // var sd = _testResolver.FindStructureDefinition(@"http://example.com/fhir/StructureDefinition/patient-research-authorization");
            // var sd = _testResolver.FindStructureDefinition(@"http://example.com/fhir/StructureDefinition/patient-legal-case");
            // var sd = _testResolver.FindStructureDefinition(@"http://hl7.org/fhir/StructureDefinition/us-core-religion");
            var sd = _testResolver.FindStructureDefinition(@"http://example.org/fhir/StructureDefinition/string-translation");

            Assert.IsNotNull(sd);
            // dumpReferences(sd);

            generateSnapshotAndCompare(sd, out StructureDefinition expanded);

            dumpOutcome(_generator.Outcome);
            dumpBasePaths(expanded);
        }


        [TestMethod]
        public void GenerateSingleSnapshot()
        {
            // var sd = _testResolver.FindStructureDefinition(@"http://hl7.org/fhir/StructureDefinition/daf-condition");
            // var sd = _testResolver.FindStructureDefinition(@"http://hl7.org/fhir/StructureDefinition/xdsdocumentreference");
            // var sd = _testResolver.FindStructureDefinition(@"http://hl7.org/fhir/StructureDefinition/gao-medicationorder");
            // var sd = _testResolver.FindStructureDefinition(@"http://hl7.org/fhir/StructureDefinition/shareablevalueset");
            // var sd = _testResolver.FindStructureDefinition(@"http://hl7.org/fhir/StructureDefinition/gao-alternate");
            // var sd = _testResolver.FindStructureDefinition(@"http://hl7.org/fhir/StructureDefinition/gao-result");
            // var sd = _testResolver.FindStructureDefinition(@"http://hl7.org/fhir/StructureDefinition/gao-procedurerequest");
            // var sd = _testResolver.FindStructureDefinition(@"http://hl7.org/fhir/StructureDefinition/cqif-guidanceartifact");

            // [WMR 20160825] Examples by Simone Heckman - custom, free-form canonical url
            // => ResourceIdentity is obsolete!
            // var sd = _testResolver.FindStructureDefinition(@"http://fhir.de/StructureDefinition/kbv/betriebsstaette");
            // var sd = _testResolver.FindStructureDefinition(@"http://fhir.de/StructureDefinition/kbv/istNebenbetriebsstaette");

            // var sd = _testResolver.FindStructureDefinition(@"http://example.org/fhir/StructureDefinition/MyBasic");

            // var sd = _testResolver.FindStructureDefinition(@"http://example.org/fhir/StructureDefinition/MyObservation2");

            // [WMR 20161219] Problem: Composition.section element in core resource has name 'section' (b/o name reference)
            // Ambiguous... snapshot generator slicing logic cannot handle this...

            // [WMR 20161222] Example by EK from validator
            // var sd = _testResolver.FindStructureDefinition(@"http://example.org/StructureDefinition/DocumentComposition");
            // var sd = _testResolver.FindStructureDefinition(@"http://hl7.org/fhir/StructureDefinition/Composition");

            // [WMR 20170110] Test problematic extension
            // var sd = _testResolver.FindStructureDefinition(@"http://hl7.org/fhir/StructureDefinition/us-core-direct");

            var sd = _testResolver.FindStructureDefinition(@"http://hl7.org/fhir/StructureDefinition/Account");

            // var sd = _testResolver.FindStructureDefinition(@"http://example.org/fhir/StructureDefinition/PatientWithExtension");

            Assert.IsNotNull(sd);

            // dumpReferences(sd);

            StructureDefinition expanded;
            generateSnapshotAndCompare(sd, out expanded);

            dumpOutcome(_generator.Outcome);
            // dumpBasePaths(expanded);
            dumpElements(expanded.Snapshot.Element);
        }

        [TestMethod]
        public void TestChoiceTypeWithMultipleProfileConstraints()
        {
            // [WMR 20161005] The following profile defines several type constraints on Observation.value[x]
            // - Type = Quantity, Profile = WeightQuantity
            // - Type = Quantity, Profile = HeightQuantity
            // - Type = string
            // The snapshot generator should support this without any issues.

            // var tempPath = Path.GetTempPath();
            // var validationTestProfiles = (new Validation.TestProfileArtifactSource()).TestProfiles;
            // var sdHeightQty = validationTestProfiles.FirstOrDefault(s => s.Url == "http://validationtest.org/fhir/StructureDefinition/HeightQuantity");
            // File.WriteAllText(Path.Combine(tempPath, "HeightQuantity.StructureDefinition.xml"), FhirSerializer.SerializeResourceToXml(sdHeightQty));
            // var sdWeightQty = validationTestProfiles.FirstOrDefault(s => s.Url == "http://validationtest.org/fhir/StructureDefinition/WeightQuantity");
            // File.WriteAllText(Path.Combine(tempPath, "WeightQuantity.StructureDefinition.xml"), FhirSerializer.SerializeResourceToXml(sdWeightQty));

            var sd = _testResolver.FindStructureDefinition(@"http://validationtest.org/fhir/StructureDefinition/WeightHeightObservation");

            Assert.IsNotNull(sd);

            // dumpReferences(sd);

            StructureDefinition expanded;
            generateSnapshotAndCompare(sd, out expanded);

            dumpOutcome(_generator.Outcome);
            dumpBasePaths(expanded);
        }

        [TestMethod]
        public void GenerateRepeatedSnapshot()
        {
            // [WMR 20161005] This generated exceptions in an early version of the snapshot generator (fixed)

            StructureDefinition expanded;
            var sd = _testResolver.FindStructureDefinition(@"http://hl7.org/fhir/StructureDefinition/MeasureReport");
            generateSnapshotAndCompare(sd, out expanded);
            dumpOutcome(_generator.Outcome);
            dumpBasePaths(expanded);

            sd = _testResolver.FindStructureDefinition(@"http://hl7.org/fhir/StructureDefinition/clinicaldocument");
            generateSnapshotAndCompare(sd, out expanded);
            dumpOutcome(_generator.Outcome);
            dumpBasePaths(expanded);
        }


        [TestMethod]
        public void TestExpandAllComplexElements()
        {
            // [WMR 20161005] This simulates custom Forge post-processing logic
            // i.e. perform a regular snapshot expansion, then explicitly expand all complex elements (esp. those without any differential constraints)

            var sd = _testResolver.FindStructureDefinition(@"http://hl7.org/fhir/StructureDefinition/Patient");
            Assert.IsNotNull(sd);
            generateSnapshot(sd);
            Assert.IsTrue(sd.HasSnapshot);
            var elems = sd.Snapshot.Element;
            Assert.AreEqual("Patient.identifier", elems[9].Path);
            Assert.AreEqual("Patient.active", elems[10].Path);
            var expanded = expandAllComplexElements(sd.Snapshot.Element);
            Assert.IsNotNull(expanded);

            var tempPath = Path.GetTempPath();
            var sdSave = (StructureDefinition)sd.DeepCopy();
            sdSave.Snapshot.Element = expanded.ToList();
            File.WriteAllText(Path.Combine(tempPath, "snapshotgen-dest.xml"), FhirSerializer.SerializeResourceToXml(sdSave));

            foreach (var elem in expanded)
            {
                Debug.WriteLine("{0}  |  {1}", elem.Path, elem.Base != null ? elem.Base.Path : null);
            }

            int i = expanded.FindIndex(e => e.Path == "Patient.identifier");
            Assert.IsTrue(i > -1);
            // Assert.AreEqual("Patient.identifier", expanded[++i].Path);
            Assert.AreEqual("Patient.identifier.id", expanded[++i].Path);
            Assert.AreEqual("Patient.identifier.extension", expanded[++i].Path);
            Assert.AreEqual("Patient.identifier.use", expanded[++i].Path);
            Assert.AreEqual("Patient.identifier.type", expanded[++i].Path);
            Assert.AreEqual("Patient.identifier.type.id", expanded[++i].Path);
            Assert.AreEqual("Patient.identifier.type.extension", expanded[++i].Path);
            Assert.AreEqual("Patient.identifier.type.coding", expanded[++i].Path);
            Assert.AreEqual("Patient.identifier.type.coding.id", expanded[++i].Path);
            Assert.AreEqual("Patient.identifier.type.coding.extension", expanded[++i].Path);
            Assert.AreEqual("Patient.identifier.type.coding.system", expanded[++i].Path);
            Assert.AreEqual("Patient.identifier.type.coding.version", expanded[++i].Path);
            Assert.AreEqual("Patient.identifier.type.coding.code", expanded[++i].Path);
            Assert.AreEqual("Patient.identifier.type.coding.display", expanded[++i].Path);
            Assert.AreEqual("Patient.identifier.type.coding.userSelected", expanded[++i].Path);
            Assert.AreEqual("Patient.identifier.type.text", expanded[++i].Path);
            Assert.AreEqual("Patient.identifier.system", expanded[++i].Path);
            Assert.AreEqual("Patient.identifier.value", expanded[++i].Path);
            Assert.AreEqual("Patient.identifier.period", expanded[++i].Path);
            Assert.AreEqual("Patient.identifier.period.id", expanded[++i].Path);
            Assert.AreEqual("Patient.identifier.period.extension", expanded[++i].Path);
            Assert.AreEqual("Patient.identifier.period.start", expanded[++i].Path);
            Assert.AreEqual("Patient.identifier.period.end", expanded[++i].Path);
            Assert.AreEqual("Patient.identifier.assigner", expanded[++i].Path);
            Assert.AreEqual("Patient.identifier.assigner.id", expanded[++i].Path);
            Assert.AreEqual("Patient.identifier.assigner.extension", expanded[++i].Path);
            Assert.AreEqual("Patient.identifier.assigner.reference", expanded[++i].Path);
            Assert.AreEqual("Patient.identifier.assigner.display", expanded[++i].Path);

            for (int j = 1; j < expanded.Count; j++)
            {
                if (isExpandableElement(expanded[j]))
                {
                    verifyExpandElement(expanded[j], elems, expanded);
                }
            }
        }

        IList<ElementDefinition> expandAllComplexElements(IList<ElementDefinition> elements)
        {
            var nav = new ElementDefinitionNavigator(elements);
            // Skip root element
            if (nav.MoveToFirstChild() && nav.MoveToFirstChild())
            {
                if (_generator == null)
                {
                    _generator = new SnapshotGenerator(_testResolver, _settings);
                }
                expandAllComplexChildElements(nav);
                return nav.Elements;
            }
            return elements;
        }

        void expandAllComplexChildElements(ElementDefinitionNavigator nav)
        {
            do
            {
                Debug.Print("[expandAllComplexChildElements] " + nav.Path);
                if (nav.HasChildren || (isExpandableElement(nav.Current) && _generator.ExpandElement(nav)))
                {
                    var bm = nav.Bookmark();
                    if (nav.MoveToFirstChild())
                    {
                        expandAllComplexChildElements(nav);
                        Assert.IsTrue(nav.ReturnToBookmark(bm));
                    }
                }
            } while (nav.MoveToNext());
        }

        bool isExpandableElement(ElementDefinition element)
        {
            var type = element.PrimaryType();
            var typeCode = type?.Code;
            return !String.IsNullOrEmpty(typeCode)
                   && element.Type.Count == 1
                   && typeCode != FHIRAllTypes.BackboneElement.GetLiteral()
                   && ModelInfo.IsDataType(typeCode)
                   && (
                        // Only expand extension elements with a custom name or profile
                        // Do NOT expand the core Extension.extension element, as this will trigger infinite recursion
                        typeCode != FHIRAllTypes.Extension.GetLiteral()
                        || !string.IsNullOrEmpty(type.Profile)
                        || element.SliceName != null
                   );
        }

        [TestMethod]
        public void TestExpandAllComplexElementsWithEvent()
        {
            // [WMR 20170105] New - hook new BeforeExpand event in order to force full expansion of all complex elements
            // Note: BeforeExpandElement is only raised for diff constraints, not for all snapshot elements...!
            // => Cannot use this to fully expand a sparse diff
            // => first generate regular snapshot, then re-run on result to expand all

            var sd = _testResolver.FindStructureDefinition(@"http://hl7.org/fhir/StructureDefinition/Patient");
            // var sd = _testResolver.FindStructureDefinition(@"http://example.org/fhir/StructureDefinition/PatientWithCustomIdentifier");

            Assert.IsNotNull(sd);

            // generateSnapshot(sd);
            _generator = new SnapshotGenerator(_testResolver, _settings);
            _generator.BeforeExpandElement += beforeExpandElementHandler;
            StructureDefinition expanded = null;
            try
            {
                generateSnapshotAndCompare(sd, out expanded);
            }
            finally
            {
                _generator.BeforeExpandElement -= beforeExpandElementHandler;
            }

            Assert.IsNotNull(expanded);
            Assert.IsTrue(expanded.HasSnapshot);
            var elems = expanded.Snapshot.Element;

            foreach (var elem in elems)
            {
                Debug.WriteLine("{0}  |  {1}", elem.Path, elem.Base?.Path);
            }

            int i = elems.FindIndex(e => e.Path == "Patient.identifier");
            Assert.IsTrue(i > -1);
            // Assert.AreEqual("Patient.identifier", expanded[++i].Path);
            Assert.AreEqual("Patient.identifier.id", elems[++i].Path);
            Assert.AreEqual("Patient.identifier.extension", elems[++i].Path);
            Assert.AreEqual("Patient.identifier.use", elems[++i].Path);
            Assert.AreEqual("Patient.identifier.type", elems[++i].Path);
            Assert.AreEqual("Patient.identifier.type.id", elems[++i].Path);
            Assert.AreEqual("Patient.identifier.type.extension", elems[++i].Path);
            Assert.AreEqual("Patient.identifier.type.coding", elems[++i].Path);
            Assert.AreEqual("Patient.identifier.type.coding.id", elems[++i].Path);
            Assert.AreEqual("Patient.identifier.type.coding.extension", elems[++i].Path);
            Assert.AreEqual("Patient.identifier.type.coding.system", elems[++i].Path);
            Assert.AreEqual("Patient.identifier.type.coding.version", elems[++i].Path);
            Assert.AreEqual("Patient.identifier.type.coding.code", elems[++i].Path);
            Assert.AreEqual("Patient.identifier.type.coding.display", elems[++i].Path);
            Assert.AreEqual("Patient.identifier.type.coding.userSelected", elems[++i].Path);
            Assert.AreEqual("Patient.identifier.type.text", elems[++i].Path);
            Assert.AreEqual("Patient.identifier.system", elems[++i].Path);
            Assert.AreEqual("Patient.identifier.value", elems[++i].Path);
            Assert.AreEqual("Patient.identifier.period", elems[++i].Path);
            Assert.AreEqual("Patient.identifier.period.id", elems[++i].Path);
            Assert.AreEqual("Patient.identifier.period.extension", elems[++i].Path);
            Assert.AreEqual("Patient.identifier.period.start", elems[++i].Path);
            Assert.AreEqual("Patient.identifier.period.end", elems[++i].Path);
            Assert.AreEqual("Patient.identifier.assigner", elems[++i].Path);
            Assert.AreEqual("Patient.identifier.assigner.id", elems[++i].Path);
            Assert.AreEqual("Patient.identifier.assigner.extension", elems[++i].Path);
            Assert.AreEqual("Patient.identifier.assigner.reference", elems[++i].Path);
            Assert.AreEqual("Patient.identifier.assigner.display", elems[++i].Path);

            for (int j = 1; j < elems.Count; j++)
            {
                if (isExpandableElement(elems[j]))
                {
                    verifyExpandElement(elems[j], elems, elems);
                }
            }
        }

        [TestMethod]
        public void TestCoreOrganizationNL()
        {
            // core-organization-nl references extension core-address-nl
            // BUG: expanded extension child elements have incorrect .Base.Path ...?!
            // e.g. Organization.address.type - Base = Organization.address.use
            // Fixed by adding conditional to copyChildren

            var sd = _testResolver.FindStructureDefinition(@"http://fhir.nl/fhir/StructureDefinition/nl-core-organization");
            Assert.IsNotNull(sd);

            _generator = new SnapshotGenerator(_testResolver, _settings);
            _generator.PrepareElement += elementHandler;
            _generator.BeforeExpandElement += beforeExpandElementHandler;
            StructureDefinition expanded = null;
            try
            {
                generateSnapshotAndCompare(sd, out expanded);
            }
            finally
            {
                _generator.PrepareElement -= elementHandler;
                _generator.BeforeExpandElement -= beforeExpandElementHandler;
            }

            Assert.IsNotNull(expanded);
            Assert.IsTrue(expanded.HasSnapshot);
            var elems = expanded.Snapshot.Element;

            foreach (var elem in elems)
            {
                Debug.WriteLine("{0}  |  {1}", elem.Path, elem.Base?.Path);
            }

            for (int j = 1; j < elems.Count; j++)
            {
                // [WMR 20170306] Problem: isExpandableElement now receives the already merged snapshot element
                // Result may now be different than before, e.g. because type has been merged
                // HACK: Explicitly exclude Organization.type (no child constraints in diff)

                if (isExpandableElement(elems[j])
                    && elems[j].Path != "Organization.type")
                {
                    verifyExpandElement(elems[j], elems, elems);
                }
            }
        }

        void beforeExpandElementHandler(object sender, SnapshotExpandElementEventArgs e)
        {
            var isExpandable = isExpandableElement(e.Element);

            Debug.Print("[beforeExpandElementHandler] #{0} '{1}' - HasChildren = {2} - MustExpand = {3} => {4}"
                .FormatWith(e.Element.GetHashCode(), e.Element.Path, e.HasChildren, e.MustExpand, isExpandable));

            // Never clear flag if already set by snapshot generator...!
            e.MustExpand |= isExpandable;
        }

        [TestMethod]
        public void TestSnapshotRecursionChecker()
        {
            // Following structuredefinition has a recursive element type profile
            // Verify that the snapshot generator detects recursion and aborts with exception

            var sd = _testResolver.FindStructureDefinition(@"http://example.org/fhir/StructureDefinition/MyBundle");

            Assert.IsNotNull(sd);

            // dumpReferences(sd);

            StructureDefinition expanded;
            bool exceptionRaised = false;
            try
            {
                generateSnapshotAndCompare(sd, out expanded);
                dumpOutcome(_generator.Outcome);
                dumpBasePaths(expanded);
            }
            catch (Exception ex)
            {
                Debug.Print("{0}: {1}".FormatWith(ex.GetType().Name, ex.Message));
                exceptionRaised = ex is NotSupportedException;
            }
            Assert.IsTrue(exceptionRaised);
        }

        [TestMethod]
        public void GenerateDerivedProfileSnapshot()
        {
            // [WMR 20161005] Verify that the snapshot generator supports profiles on profiles

            // cqif-guidanceartifact profile is derived from cqif-knowledgemodule
            // var sd = _testResolver.FindStructureDefinition(@"http://hl7.org/fhir/StructureDefinition/cqif-guidanceartifact");
            // var sd = _testResolver.FindStructureDefinition(@"http://hl7.org/fhir/StructureDefinition/sdc-questionnaire");
            // var sd = _testResolver.FindStructureDefinition(@"http://hl7.org/fhir/StructureDefinition/qicore-goal");
            // var sd = _testResolver.FindStructureDefinition(@"http://hl7.org/fhir/StructureDefinition/qicore-patient");
            var sd = _testResolver.FindStructureDefinition(@"http://hl7.org/fhir/StructureDefinition/qicore-encounter");

            Assert.IsNotNull(sd);
            // dumpReferences(sd);

            StructureDefinition expanded;
            generateSnapshotAndCompare(sd, out expanded);

            dumpOutcome(_generator.Outcome);
            dumpBasePaths(expanded);
        }

        void assertContainsElement(StructureDefinition sd, string path, string name = null, string elementId = null)
        {
            Assert.IsNotNull(sd);

            Assert.IsNotNull(sd.Differential);
            Assert.IsNotNull(sd.Differential.Element);
            Assert.IsTrue(sd.Differential.Element.Count > 0);

            // Verify that the differential component contains a matching element
            assertContainsElement(sd.Differential, path, name);
            assertContainsElement(sd.Snapshot, path, name, elementId);
        }

        void assertContainsElement(IElementList elements, string path, string name = null, string elementId = null)
        {
            var label = elements is StructureDefinition.DifferentialComponent ? "differential" : "snapshot";
            Assert.IsNotNull(elements);
            var matches = elements.Element.Where(e => e.Path == path && e.SliceName == name).ToArray();
            var cnt = matches.Length;
            Assert.IsTrue(cnt > 0, $"Expected element is missing from {label} component. Path = '{path}', name = '{name}'.");
            Assert.IsTrue(cnt == 1, $"Found multiple matching elements in {label} component for Path = '{path}', name = '{name}'.");
            var elem = matches[0];
            if (_settings.GenerateElementIds && elementId != null)
            {
                Assert.AreEqual(elementId, elem.ElementId, $"Invalid elementId in {label} component. Expected = '{elementId}', actual = '{elem.ElementId}'.");
            }
        }

        StructureDefinition generateSnapshot(string url, Action<StructureDefinition> preprocessor = null)
        {
            StructureDefinition expanded = null;
            var structure = _testResolver.FindStructureDefinition(url);
            Assert.IsNotNull(structure);
            Assert.IsTrue(structure.HasSnapshot);
            preprocessor?.Invoke(structure);
            generateSnapshotAndCompare(structure, out expanded);
            dumpOutcome(_generator.Outcome);
            return expanded;
        }

        static void ensure(StructureDefinition structure, ElementDefinition insertBefore, params ElementDefinition[] inserts)
            => ensure(structure.Differential.Element, insertBefore, inserts);

        static void ensure(List<ElementDefinition> elements, ElementDefinition insertBefore, params ElementDefinition[] inserts)
        {
            var idx = elements.FindIndex(e => e.Path == insertBefore.Path && e.SliceName == insertBefore.SliceName);
            Assert.AreNotEqual(-1, idx, $"Warning! insertBefore element is missing. Path = '{insertBefore.Path}', Name = '{insertBefore.SliceName}'.");
            foreach (var insert in inserts)
            {
                var idx2 = elements.FindIndex(e => e.Path == insert.Path && e.SliceName == insert.SliceName);
                Assert.AreEqual(-1, idx2, $"Warning! insert element is already present. Path = '{insert.Path}', Name = '{insert.SliceName}'.");
            }
            elements.InsertRange(idx, inserts);
        }

        [TestMethod]
        public void GeneratePatientWithExtensionsSnapshot()
        {
            // [WMR 20161005] Very complex set of examples by Chris Grenz
            // https://github.com/chrisgrenz/FHIR-Primer/blob/master/profiles/patient-extensions-profile.xml
            // Manually downgraded from FHIR v1.4.0 to v1.0.2

            StructureDefinition sd;
            ElementVerifier verifier;

            _settings.GenerateElementIds = true;

#if true
            // http://example.com/fhir/StructureDefinition/patient-legal-case
            // http://example.com/fhir/StructureDefinition/patient-legal-case-lead-counsel

            // Verify complex extension used by patient-with-extensions profile
            // patient-research-authorization-profile.xml
            sd = generateSnapshot(@"http://example.com/fhir/StructureDefinition/patient-research-authorization");
            verifier = new ElementVerifier(sd, _settings);
            verifier.VerifyElement("Extension.extension", null, "Extension.extension");
            verifier.VerifyElement("Extension.extension", "type", "Extension.extension:type");
            verifier.VerifyElement("Extension.extension.url", "type.url", "Extension.extension:type.url", new FhirUri("type"));
            verifier.VerifyElement("Extension.extension", "flag", "Extension.extension:flag");
            verifier.VerifyElement("Extension.extension.url", "flag.url", "Extension.extension:flag.url", new FhirUri("flag"));
            verifier.VerifyElement("Extension.extension", "date", "Extension.extension:date");
            verifier.VerifyElement("Extension.extension.url", "date.url", "Extension.extension:date.url", new FhirUri("date"));
            verifier.VerifyElement("Extension.url", null, "Extension.url", new FhirUri(sd.Url));

            // Basic Patient profile that references a set of extensions
            // patient-extensions-profile.xml
            sd = generateSnapshot(@"http://example.com/fhir/SD/patient-with-extensions");
            verifier = new ElementVerifier(sd, _settings);
            verifier.VerifyElement("Patient.extension", null, "Patient.extension");
            verifier.VerifyElement("Patient.extension", "doNotCall", "Patient.extension:doNotCall");
            verifier.VerifyElement("Patient.extension", "legalCase", "Patient.extension:legalCase");
            verifier.VerifyElement("Patient.extension.valueBoolean", "legalCase.valueBoolean", "Patient.extension:legalCase.valueBoolean");
            verifier.VerifyElement("Patient.extension.valueBoolean.extension", null, "Patient.extension:legalCase.valueBoolean.extension");
            verifier.VerifyElement("Patient.extension.valueBoolean.extension", "legalCase.valueBoolean.leadCounsel", "Patient.extension:legalCase.valueBoolean.extension:leadCounsel");
            verifier.VerifyElement("Patient.extension", "religion", "Patient.extension:religion");
            verifier.VerifyElement("Patient.extension", "researchAuth", "Patient.extension:researchAuth");

            // Each of the following profiles is derived from the previous profile

            // patient-name-slice-profile.xml
            sd = generateSnapshot(@"http://example.com/fhir/SD/patient-name-slice"
                , structure => ensure(structure,
                     new ElementDefinition() { Path = "Patient.name.use", SliceName = "maidenName.use" },
                     // Add named parent slicing entry
                     new ElementDefinition() { Path = "Patient.name", SliceName = "maidenName" }
                 )
            );
            verifier = new ElementVerifier(sd, _settings);
            verifier.VerifyElement("Patient.name", null, "Patient.name");
            verifier.VerifyElement("Patient.name", "officialName", "Patient.name:officialName");
            verifier.VerifyElement("Patient.name.text", "officialName.text", "Patient.name:officialName.text");
            verifier.VerifyElement("Patient.name.family", "officialName.family", "Patient.name:officialName.family");
            verifier.VerifyElement("Patient.name.given", "officialName.given", "Patient.name:officialName.given");
            verifier.VerifyElement("Patient.name.use", "officialName.use", "Patient.name:officialName.use");
            Assert.AreEqual((verifier.Current.Fixed as Code)?.Value, "official");
            verifier.VerifyElement("Patient.name", "maidenName", "Patient.name:maidenName");
            verifier.VerifyElement("Patient.name.use", "maidenName.use", "Patient.name:maidenName.use");
            Assert.AreEqual((verifier.Current.Fixed as Code)?.Value, "maiden");
            verifier.VerifyElement("Patient.name.family", "maidenName.family", "Patient.name:maidenName.family");

            // patient-telecom-slice-profile.xml
            sd = generateSnapshot(@"http://example.com/fhir/SD/patient-telecom-slice"
                , structure => ensure(structure,
                     new ElementDefinition() { Path = "Patient.telecom.system", SliceName = "workEmail.system" },
                     // Add named parent slicing entry
                     new ElementDefinition() { Path = "Patient.telecom", SliceName = "workEmail" }
                 )
            );
            verifier = new ElementVerifier(sd, _settings);
            verifier.VerifyElement("Patient.telecom", null, "Patient.telecom");
            verifier.VerifyElement("Patient.telecom", "homePhone", "Patient.telecom:homePhone");
            verifier.VerifyElement("Patient.telecom.system", "homePhone.system", "Patient.telecom:homePhone.system", new Code("phone"));
            verifier.VerifyElement("Patient.telecom.use", "homePhone.use", "Patient.telecom:homePhone.use", new Code("home"));
            verifier.VerifyElement("Patient.telecom", "mobilePhone", "Patient.telecom:mobilePhone");
            verifier.VerifyElement("Patient.telecom.system", "mobilePhone.system", "Patient.telecom:mobilePhone.system", new Code("phone"));
            verifier.VerifyElement("Patient.telecom.use", "mobilePhone.use", "Patient.telecom:mobilePhone.use", new Code("mobile"));
            verifier.VerifyElement("Patient.telecom", "homeEmail", "Patient.telecom:homeEmail");
            verifier.VerifyElement("Patient.telecom.system", "homeEmail.system", "Patient.telecom:homeEmail.system", new Code("email"));
            verifier.VerifyElement("Patient.telecom.use", "homeEmail.use", "Patient.telecom:homeEmail.use", new Code("home"));
            verifier.VerifyElement("Patient.telecom", "workEmail", "Patient.telecom:workEmail");
            verifier.VerifyElement("Patient.telecom.system", "workEmail.system", "Patient.telecom:workEmail.system", new Code("email"));
            verifier.VerifyElement("Patient.telecom.use", "workEmail.use", "Patient.telecom:workEmail.use", new Code("work"));
            verifier.VerifyElement("Patient.telecom", "pager", "Patient.telecom:pager");
            verifier.VerifyElement("Patient.telecom.system", "pager.system", "Patient.telecom:pager.system", new Code("pager"));

            // Original snapshot contains constraints for both deceased[x] and deceasedDateTime - invalid!
            // Generated snapshot merges both constraints to deceasedDateTime type slice
            // patient-deceasedDatetime-slice-profile.xml
            sd = generateSnapshot(@"http://example.com/fhir/SD/patient-deceasedDatetime-slice");
            assertContainsElement(sd.Differential, "Patient.deceased[x]");                  // Differential contains a type slice on deceased[x]
            // Assert.IsFalse(sd.Snapshot.Element.Any(e => e.Path == "Patient.deceased[x]"));  // Snapshot only contains renamed element constraint
            // assertContainsElement(sd, "Patient.deceasedDateTime", null, "Patient.deceasedDateTime");
            verifier.VerifyElement("Patient.deceased[x]", null, "Patient.deceased[x]");

            // patient-careprovider-type-slice-profile.xml
            sd = generateSnapshot(@"http://example.com/fhir/SD/patient-careprovider-type-slice");
            verifier = new ElementVerifier(sd, _settings);
            verifier.VerifyElement("Patient.careProvider", null, "Patient.careProvider");
            verifier.VerifyElement("Patient.careProvider", "organizationCare", "Patient.careProvider:organizationCare");
            verifier.VerifyElement("Patient.careProvider", "practitionerCare", "Patient.careProvider:practitionerCare");

            // Verify re-slicing
            // patient-careprovider-type-reslice-profile.xml
            sd = generateSnapshot(@"http://example.com/fhir/SD/patient-careprovider-type-reslice");
            verifier = new ElementVerifier(sd, _settings);
            verifier.VerifyElement("Patient.careProvider", null, "Patient.careProvider");
            verifier.VerifyElement("Patient.careProvider", "organizationCare", "Patient.careProvider:organizationCare");
            verifier.VerifyElement("Patient.careProvider", "organizationCare/teamCare", "Patient.careProvider:organizationCare/teamCare");
            verifier.VerifyElement("Patient.careProvider", "practitionerCare", "Patient.careProvider:practitionerCare");

            // Identifier Datatype profile
            // patient-mrn-id-profile.xml
            sd = generateSnapshot(@"http://example.com/fhir/SD/patient-mrn-id");
            verifier = new ElementVerifier(sd, _settings);
            verifier.VerifyElement("Identifier", null, "Identifier");
            verifier.VerifyElement("Identifier.system", null, "Identifier.system", new FhirUri(@"http://example.com/fhir/localsystems/PATIENT-ID-MRN"));

            // Verify inline re-slicing
            // Profile slices identifier and also re-slices the "mrn" slice
            // patient-identifier-profile-slice-profile.xml
            sd = generateSnapshot(@"http://example.com/fhir/SD/patient-slice-by-profile"
                , structure => ensure(structure,
                     new ElementDefinition() { Path = "Patient.identifier.use", SliceName = "mrn/officialMRN.use" },
                     // Add named parent reslicing entry
                     new ElementDefinition() { Path = "Patient.identifier", SliceName = "mrn/officialMRN" }
                 )
            );
            verifier = new ElementVerifier(sd, _settings);
            verifier.VerifyElement("Patient.identifier", null, "Patient.identifier");
            verifier.VerifyElement("Patient.identifier", "mrn", "Patient.identifier:mrn");
            verifier.VerifyElement("Patient.identifier", "mrn/officialMRN", "Patient.identifier:mrn/officialMRN");
            verifier.VerifyElement("Patient.identifier.use", "mrn/officialMRN.use", "Patient.identifier:mrn/officialMRN.use", new Code("official"));
            verifier.VerifyElement("Patient.identifier", "mdmId", "Patient.identifier:mdmId");

            // Verify constraints on named slice in base profile
            // patient-identifier-slice-extension-profile.xml
            sd = generateSnapshot(@"http://example.com/fhir/SD/patient-identifier-subslice"
                , structure => ensure(structure,
                     new ElementDefinition() { Path = "Patient.identifier.extension", SliceName = "mrn.issuingSite" },
                     // Add named parent reslicing entry
                     new ElementDefinition() { Path = "Patient.identifier", SliceName = "mrn" }
                 )
            );
            verifier = new ElementVerifier(sd, _settings);
            verifier.VerifyElement("Patient.identifier", null, "Patient.identifier");
            verifier.AssertSlicing(new string[] { "system" }, ElementDefinition.SlicingRules.Open, null);
            verifier.VerifyElement("Patient.identifier", "mrn", "Patient.identifier:mrn");
            verifier.AssertSlicing(new string[] { "use" }, ElementDefinition.SlicingRules.Open, null);
            verifier.VerifyElement("Patient.identifier.extension", null, "Patient.identifier:mrn.extension");
            verifier.VerifyElement("Patient.identifier.extension", "mrn.issuingSite", "Patient.identifier:mrn.extension:issuingSite");
            verifier.VerifyElement("Patient.identifier.use", null, "Patient.identifier:mrn.use");
            verifier.VerifyElement("Patient.identifier.type", null, "Patient.identifier:mrn.type");
            verifier.VerifyElement("Patient.identifier.system", null, "Patient.identifier:mrn.system", new FhirUri(@"http://example.com/fhir/localsystems/PATIENT-ID-MRN"));
            verifier.VerifyElement("Patient.identifier.value", null, "Patient.identifier:mrn.value");
            verifier.VerifyElement("Patient.identifier.period", null, "Patient.identifier:mrn.period");
            verifier.VerifyElement("Patient.identifier.assigner", null, "Patient.identifier:mrn.assigner");
            verifier.VerifyElement("Patient.identifier", "mrn/officialMRN", "Patient.identifier:mrn/officialMRN");
            verifier.VerifyElement("Patient.identifier", "mdmId", "Patient.identifier:mdmId");
#endif

            // Verify extension re-slice
            // patient-research-auth-reslice-profile.xml
            sd = generateSnapshot(@"http://example.com/fhir/SD/patient-research-auth-reslice"
                , structure => ensure(structure,
                     new ElementDefinition() { Path = "Patient.extension.extension.value[x]", SliceName = "researchAuth/grandfatheredResAuth.type.value[x]" },
                     // Add named parent reslicing entry
                     new ElementDefinition() { Path = "Patient.extension", SliceName = "researchAuth/grandfatheredResAuth" },
                     new ElementDefinition() { Path = "Patient.extension.extension", SliceName = "type" }
                     // new ElementDefinition() { Path = "Patient.extension.extension", Name = "researchAuth/grandfatheredResAuth.type" }
                 )
            );
            verifier = new ElementVerifier(sd, _settings);
            verifier.VerifyElement("Patient.extension", null, "Patient.extension");
            verifier.VerifyElement("Patient.extension", "doNotCall", "Patient.extension:doNotCall");
            verifier.VerifyElement("Patient.extension", "legalCase", "Patient.extension:legalCase");
            verifier.VerifyElement("Patient.extension.valueBoolean", "legalCase.valueBoolean", "Patient.extension:legalCase.valueBoolean");
            verifier.VerifyElement("Patient.extension.valueBoolean.extension", null, "Patient.extension:legalCase.valueBoolean.extension");
            verifier.VerifyElement("Patient.extension.valueBoolean.extension", "legalCase.valueBoolean.leadCounsel", "Patient.extension:legalCase.valueBoolean.extension:leadCounsel");
            verifier.VerifyElement("Patient.extension", "religion", "Patient.extension:religion");
            verifier.VerifyElement("Patient.extension", "researchAuth", "Patient.extension:researchAuth");
            // Note: in the original snapshot, the "researchAuth" complex extension slice is fully expanded (child extensions: type, flag, date)
            // However this is not necessary, as there are no child constraints on the extension

            // [WMR 20161216] TODO: Merge slicing entry
            verifier.AssertSlicing(new string[] { "type.value[x]" }, ElementDefinition.SlicingRules.Open, null);

            // [WMR 20161208] TODO...

            // "researchAuth/grandfatheredResAuth" represents a reslice of the base extension "researchAuth" (0...*)
            verifier.VerifyElement("Patient.extension", "researchAuth/grandfatheredResAuth", "Patient.extension:researchAuth/grandfatheredResAuth");

            // [WMR 20161216] TODO: Merge slicing entry
            verifier.VerifyElement("Patient.extension.extension", null, "Patient.extension:researchAuth/grandfatheredResAuth.extension");
            verifier.AssertSlicing(new string[] { "url" }, ElementDefinition.SlicingRules.Open, false);

            // The reslice "researchAuth/grandfatheredResAuth" has a child element constraint on "type.value[x]"
            // Therefore the complex extension is fully expanded (child extensions: type, flag, date)
            verifier.VerifyElement("Patient.extension.extension", "type", "Patient.extension:researchAuth/grandfatheredResAuth.extension:type");
            verifier.VerifyElement("Patient.extension.extension.url", null, "Patient.extension:researchAuth/grandfatheredResAuth.extension:type.url", new FhirUri("type"));
            // Child constraints on "type.value[x]" merged from differential
            verifier.VerifyElement("Patient.extension.extension.value[x]", "researchAuth/grandfatheredResAuth.type.value[x]", "Patient.extension:researchAuth/grandfatheredResAuth.extension:type.value[x]");
            verifier.VerifyElement("Patient.extension.extension", "flag", "Patient.extension:researchAuth/grandfatheredResAuth.extension:flag");
            verifier.VerifyElement("Patient.extension.extension.url", null, "Patient.extension:researchAuth/grandfatheredResAuth.extension:flag.url", new FhirUri("flag"));
            verifier.VerifyElement("Patient.extension.extension", "date", "Patient.extension:researchAuth/grandfatheredResAuth.extension:date");
            verifier.VerifyElement("Patient.extension.extension.url", null, "Patient.extension:researchAuth/grandfatheredResAuth.extension:date.url", new FhirUri("date"));
            verifier.VerifyElement("Patient.extension.url", null, "Patient.extension:researchAuth/grandfatheredResAuth.url", new FhirUri(@"http://example.com/fhir/StructureDefinition/patient-research-authorization"));

            // Slices inherited from base profile with url http://example.com/fhir/SD/patient-identifier-subslice
            verifier.VerifyElement("Patient.identifier", null, "Patient.identifier");
            verifier.AssertSlicing(new string[] { "system" }, ElementDefinition.SlicingRules.Open, null);
            verifier.VerifyElement("Patient.identifier", "mrn", "Patient.identifier:mrn");
            verifier.AssertSlicing(new string[] { "use" }, ElementDefinition.SlicingRules.Open, null);
            verifier.VerifyElement("Patient.identifier.extension", null, "Patient.identifier:mrn.extension");
            verifier.VerifyElement("Patient.identifier.extension", "mrn.issuingSite", "Patient.identifier:mrn.extension:issuingSite");
            verifier.VerifyElement("Patient.identifier.use", null, "Patient.identifier:mrn.use");
            verifier.VerifyElement("Patient.identifier.type", null, "Patient.identifier:mrn.type");
            verifier.VerifyElement("Patient.identifier.system", null, "Patient.identifier:mrn.system", new FhirUri(@"http://example.com/fhir/localsystems/PATIENT-ID-MRN"));
            verifier.VerifyElement("Patient.identifier.value", null, "Patient.identifier:mrn.value");
            verifier.VerifyElement("Patient.identifier.period", null, "Patient.identifier:mrn.period");
            verifier.VerifyElement("Patient.identifier.assigner", null, "Patient.identifier:mrn.assigner");
            verifier.VerifyElement("Patient.identifier", "mrn/officialMRN", "Patient.identifier:mrn/officialMRN");
            verifier.VerifyElement("Patient.identifier", "mdmId", "Patient.identifier:mdmId");

        }

        [TestMethod]
        public void GenerateSnapshotExpandExternalProfile()
        {
            // Profile MyLocation references extension MyLocationExtension
            // MyLocationExtension extension profile does not have a snapshot component => expand on demand

            var sd = _testResolver.FindStructureDefinition(@"http://example.org/fhir/StructureDefinition/MyLocation");
            Assert.IsNotNull(sd);
            Assert.IsNotNull(sd.Snapshot);

            var extensionElements = sd.Differential.Element.Where(e => e.IsExtension());
            Assert.IsNotNull(extensionElements);
            Assert.AreEqual(2, extensionElements.Count()); // Extension slicing entry + first extension definition
            var extensionElement = extensionElements.Skip(1).FirstOrDefault();
            var extensionType = extensionElement.Type.FirstOrDefault();
            Assert.IsNotNull(extensionType);
            Assert.AreEqual(FHIRAllTypes.Extension.GetLiteral(), extensionType.Code);
            Assert.IsNotNull(extensionType.Profile);
            var extDefUrl = extensionType.Profile;
            Assert.AreEqual(@"http://example.org/fhir/StructureDefinition/MyLocationExtension", extDefUrl);
            var ext = _testResolver.FindStructureDefinition(extDefUrl);
            Assert.IsNotNull(ext);
            Assert.IsNull(ext.Snapshot);

            // dumpReferences(sd);

            StructureDefinition expanded;
            generateSnapshotAndCompare(sd, out expanded);

            dumpOutcome(_generator.Outcome);
            dumpBasePaths(expanded);
        }

        [TestMethod]
        public void GenerateSnapshotIgnoreMissingExternalProfile()
        {
            // [WMR 20161005] Verify that the snapshot generator gracefully handles unresolved external profile references
            // This should generate a partial snapshot and OperationOutcome Issues for each missing dependency.

            var sd = _testResolver.FindStructureDefinition(@"http://example.org/fhir/StructureDefinition/MyObservation");
            Assert.IsNotNull(sd);

            dumpReferences(sd, true);

            // Explicitly disable expansion of external snapshots
            var settings = new SnapshotGeneratorSettings(_settings);
            settings.GenerateSnapshotForExternalProfiles = false;
            _generator = new SnapshotGenerator(_testResolver, settings);

            StructureDefinition expanded;
            generateSnapshotAndCompare(sd, out expanded);

            var outcome = _generator.Outcome;
            dumpOutcome(outcome);

            Assert.IsNotNull(outcome);
            Assert.AreEqual(3, outcome.Issue.Count);

            assertIssue(outcome.Issue[0], Issue.UNAVAILABLE_REFERENCED_PROFILE, "http://example.org/fhir/StructureDefinition/MyMissingExtension");
            // Note: the extension reference to MyExtensionNoSnapshot should not generate an Issue,
            // as the profile only needs to merge the extension definition root element (no full expansion)
            assertIssue(outcome.Issue[1], Issue.UNAVAILABLE_REFERENCED_PROFILE, "http://example.org/fhir/StructureDefinition/MyIdentifier");
            assertIssue(outcome.Issue[2], Issue.UNAVAILABLE_REFERENCED_PROFILE, "http://example.org/fhir/StructureDefinition/MyCodeableConcept");
        }

        static void assertIssue(OperationOutcome.IssueComponent issue, Issue expected, string diagnostics = null)
        {
            Assert.IsNotNull(issue);
            Assert.AreEqual(expected.Type, issue.Code);
            Assert.AreEqual(expected.Severity, issue.Severity);
            Assert.AreEqual(expected.Code.ToString(), issue.Details.Coding[0].Code);
            Assert.IsNotNull(issue.Extension);
            if (diagnostics != null)
            {
                Assert.AreEqual(diagnostics, issue.Diagnostics);
            }
        }

        // [WMR 20160721] Following profiles are not yet handled (TODO)
        //      private readonly string[] skippedProfiles =
        //      {
        //	// Differential defines constraint on MedicationOrder.reason[x]
        //	// Snapshot renames this element to MedicationOrder.reasonCodeableConcept - is this mandatory?
        //	// @"http://hl7.org/fhir/StructureDefinition/gao-medicationorder",
        //};

        [TestMethod]
        public void GenerateSnapshot()
        {
            var sw = new Stopwatch();
            int count = 0;
            _source.Reset();
            sw.Start();

            foreach (var original in findConstraintStrucDefs()
            // [WMR 20160721] Skip invalid profiles
            // .Where(sd => !skippedProfiles.Contains(sd.Url))
            )
            {
                // nothing to test, original does not have a snapshot
                if (original.Snapshot == null) continue;

                Debug.WriteLine("Generating Snapshot for " + original.Url);

                generateSnapshotAndCompare(original);
                count++;
            }

            sw.Stop();
            _source.ShowDuration(count, sw.Elapsed);
        }

        //private void forDoc()
        //{
        //    FhirXmlParser parser = new FhirXmlParser(new ParserSettings { AcceptUnknownMembers = true });
        //    IFhirReader xmlWithPatientData = null;
        //    var patient = parser.Parse<Patient>(xmlWithPatientData);

        //    // -----

        //    ArtifactResolver source = ArtifactResolver.CreateCachedDefault();
        //    var settings = new SnapshotGeneratorSettings { IgnoreMissingTypeProfiles = true };
        //    StructureDefinition profile = null;

        //    var generator = new SnapshotGenerator(source, _settings);
        //    generator.Generate(profile);
        //}

        StructureDefinition generateSnapshot(StructureDefinition original)
        {
            if (_generator == null)
            {
                _generator = new SnapshotGenerator(_testResolver, _settings);
            }

            var expanded = (StructureDefinition)original.DeepCopy();
            Assert.IsTrue(original.IsExactly(expanded));

            _generator.Update(expanded);

            return expanded;
        }

        bool generateSnapshotAndCompare(StructureDefinition original)
        {
            StructureDefinition expanded;
            return generateSnapshotAndCompare(original, out expanded);
        }

        bool generateSnapshotAndCompare(StructureDefinition original, out StructureDefinition expanded)
        {
            expanded = generateSnapshot(original);

            var areEqual = original.IsExactly(expanded);

            // [WMR 20160803] Always save output to separate file, convenient for debugging
            // if (!areEqual)
            // {
            var tempPath = Path.GetTempPath();
            File.WriteAllText(Path.Combine(tempPath, "snapshotgen-source.xml"), FhirSerializer.SerializeResourceToXml(original));
            File.WriteAllText(Path.Combine(tempPath, "snapshotgen-dest.xml"), FhirSerializer.SerializeResourceToXml(expanded));
            // }

            // Assert.IsTrue(areEqual);
            Debug.WriteLineIf(original.HasSnapshot && !areEqual, "WARNING: '{0}' Expansion ({1} elements) is not equal to original ({2} elements)!".FormatWith(
                original.Name, original.HasSnapshot ? original.Snapshot.Element.Count : 0, expanded.HasSnapshot ? expanded.Snapshot.Element.Count : 0)
            );

            return areEqual;
        }

        IEnumerable<StructureDefinition> findConstraintStrucDefs()
        {
            var testSDs = _source.FindAll<StructureDefinition>();

            foreach (var testSD in testSDs)
            {
                var sdInfo = testSD.Annotation<OriginInformation>();
                // [WMR 20160721] Select all profiles in profiles-others.xml
                var fileName = Path.GetFileNameWithoutExtension(sdInfo.Origin);
                if (fileName == "profiles-others")
                {
                    //var sd = _testResolver.FindStructureDefinition(sdInfo.Canonical);

                    //if (sd == null) throw new InvalidOperationException(("Source listed canonical url {0} [source {1}], " +
                    //    "but could not get structure definition by that url later on!").FormatWith(sdInfo.Canonical, sdInfo.Origin));

                    if (testSD.IsConstraint || testSD.IsExtension)
                        yield return testSD;
                }
            }
        }

        // Unit tests for DifferentialTreeConstructor

        [TestMethod]
        public void TestDifferentialTree()
        {
            var e = new List<ElementDefinition>();

            e.Add(new ElementDefinition() { Path = "A.B.C1" });
            e.Add(new ElementDefinition() { Path = "A.B.C1", SliceName = "C1-A" }); // First slice of A.B.C1
            e.Add(new ElementDefinition() { Path = "A.B.C2" });
            e.Add(new ElementDefinition() { Path = "A.B", SliceName = "B-A" }); // First slice of A.B
            e.Add(new ElementDefinition() { Path = "A.B.C1.D" });
            e.Add(new ElementDefinition() { Path = "A.D.F" });

            var tree = DifferentialTreeConstructor.MakeTree(e);
            Assert.IsNotNull(tree);

            var nav = new ElementDefinitionNavigator(tree);
            Assert.AreEqual(10, nav.Count);

            Assert.IsTrue(nav.MoveToChild("A"));
            Assert.IsTrue(nav.MoveToChild("B"));
            Assert.IsTrue(nav.MoveToChild("C1"));
            Assert.IsTrue(nav.MoveToNext("C1"));
            Assert.IsTrue(nav.MoveToNext("C2"));

            Assert.IsTrue(nav.MoveToParent());  // 1st A.B
            Assert.IsTrue(nav.MoveToNext() && nav.Path == "A.B");  // (now) 2nd A.B
            Assert.IsTrue(nav.MoveToChild("C1"));
            Assert.IsTrue(nav.MoveToChild("D"));

            Assert.IsTrue(nav.MoveToParent());  // A.B.C1
            Assert.IsTrue(nav.MoveToParent());  // A.B (2nd)
            Assert.IsTrue(nav.MoveToNext() && nav.Path == "A.D");
            Assert.IsTrue(nav.MoveToChild("F"));
        }

        [TestMethod]
        public void TestDifferentialTreeMultipleRoots()
        {
            var elements = new List<ElementDefinition>();

            elements.Add(new ElementDefinition() { Path = "Patient.identifier" });
            elements.Add(new ElementDefinition() { Path = "Patient" });

            bool exceptionRaised = false;
            try
            {
                var tree = DifferentialTreeConstructor.MakeTree(elements);
            }
            catch (InvalidOperationException ex)
            {
                Debug.Print(ex.Message);
                exceptionRaised = true;
            }
            Assert.IsTrue(exceptionRaised);
        }

        // [WMR 20161012] Advanced unit test for DifferentialTreeConstructor with resliced input
        [TestMethod]
        public void TestDifferentialTreeForReslice()
        {
            var elements = new List<ElementDefinition>();

            elements.Add(new ElementDefinition() { Path = "Patient.identifier" });
            elements.Add(new ElementDefinition() { Path = "Patient.identifier", SliceName = "A" });
            elements.Add(new ElementDefinition() { Path = "Patient.identifier.use" });
            elements.Add(new ElementDefinition() { Path = "Patient.identifier", SliceName = "B/1" });
            elements.Add(new ElementDefinition() { Path = "Patient.identifier.type" });
            elements.Add(new ElementDefinition() { Path = "Patient.identifier", SliceName = "B/2" });
            elements.Add(new ElementDefinition() { Path = "Patient.identifier.period.start" });
            elements.Add(new ElementDefinition() { Path = "Patient.identifier", SliceName = "C/1" });

            var tree = DifferentialTreeConstructor.MakeTree(elements);
            Assert.IsNotNull(tree);
            Debug.Print(string.Join(Environment.NewLine, tree.Select(e => $"{e.Path} : '{e.SliceName}'")));

            Assert.AreEqual(10, tree.Count);
            var verifier = new ElementVerifier(tree, _settings);

            verifier.VerifyElement("Patient");                      // Added: root element
            verifier.VerifyElement("Patient.identifier");
            verifier.VerifyElement("Patient.identifier", "A");
            verifier.VerifyElement("Patient.identifier.use");
            verifier.VerifyElement("Patient.identifier", "B/1");
            verifier.VerifyElement("Patient.identifier.type");
            verifier.VerifyElement("Patient.identifier", "B/2");
            verifier.VerifyElement("Patient.identifier.period");    // Added: parent element
            verifier.VerifyElement("Patient.identifier.period.start");
            verifier.VerifyElement("Patient.identifier", "C/1");
        }

        [TestMethod]
        public void DebugDifferentialTree()
        {
            var sd = _testResolver.FindStructureDefinition(@"http://example.com/fhir/SD/patient-research-auth-reslice");
            Assert.IsNotNull(sd);
            var tree = DifferentialTreeConstructor.MakeTree(sd.Differential.Element);
            Assert.IsNotNull(tree);
            Debug.Print(string.Join(Environment.NewLine, tree.Select(e => $"{e.Path} : '{e.SliceName}'")));
        }

        // [WMR 20160802] Unit tests for SnapshotGenerator.ExpandElement

        // [WMR 20161005] internal expandElement method is no longer unit-testable; uninitialized recursion stack causes exceptions

        //[TestMethod]
        //public void TestExpandChild()
        //{
        //    var sd = _testResolver.FindStructureDefinitionForCoreType(FHIRAllTypes.Questionnaire);
        //    Assert.IsNotNull(sd);
        //    Assert.IsNotNull(sd.Snapshot);
        //    var nav = new ElementDefinitionNavigator(sd.Snapshot.Element);
        //
        //    var generator = new SnapshotGenerator(_testResolver, SnapshotGeneratorSettings.Default);
        //
        //    nav.JumpToFirst("Questionnaire.telecom");
        //    Assert.IsTrue(generator.expandElement(nav));
        //    Assert.IsTrue(nav.MoveToChild("period"), "Did not move into complex datatype ContactPoint");
        //
        //    nav.JumpToFirst("Questionnaire.group");
        //    Assert.IsTrue(generator.expandElement(nav));
        //    Assert.IsTrue(nav.MoveToChild("title"), "Did not move into internally defined backbone element Group");
        //}

        [TestMethod]
        public void TestExpandElement_PatientIdentifier()
        {
            testExpandElement(@"http://hl7.org/fhir/StructureDefinition/Patient", "Patient.identifier");
        }

        [TestMethod]
        public void TestExpandElement_PatientName()
        {
            testExpandElement(@"http://hl7.org/fhir/StructureDefinition/Patient", "Patient.name");
        }

        [TestMethod, Ignore]
        public void TestExpandElement_QuestionnaireGroupGroup()
        {
            // Validate name reference expansion
            testExpandElement(@"http://hl7.org/fhir/StructureDefinition/Questionnaire", "Questionnaire.item");
        }

        [TestMethod, Ignore]
        public void TestExpandElement_QuestionnaireGroupQuestionGroup()
        {
            // Validate name reference expansion
            testExpandElement(@"http://hl7.org/fhir/StructureDefinition/Questionnaire", "Questionnaire.item.item");
        }

        [TestMethod]
        public void TestExpandElement_Slice()
        {
            var sd = _testResolver.FindStructureDefinition("http://hl7.org/fhir/StructureDefinition/lipidprofile");
            Assert.IsNotNull(sd);
            Assert.IsNotNull(sd.Snapshot);

            // DiagnosticReport.result is sliced
            var nav = new ElementDefinitionNavigator(sd.Snapshot.Element);

            // Move to slicing entry
            nav.JumpToFirst("DiagnosticReport.result");
            Assert.IsNotNull(nav.Current.Slicing);

            // Move to first (named) slice
            nav.MoveToNext();
            Assert.AreEqual(nav.Path, "DiagnosticReport.result");
            Assert.IsNotNull(nav.Current.SliceName);

            testExpandElement(sd, nav.Current);
        }

        void testExpandElement(string srcProfileUrl, string expandElemPath)
        {
            // Prepare...
            var sd = _testResolver.FindStructureDefinition(srcProfileUrl);
            Assert.IsNotNull(sd);
            Assert.IsNotNull(sd.Snapshot);

            var elems = sd.Snapshot.Element;
            Assert.IsNotNull(elems);

            Debug.WriteLine("Input:");
            Debug.Indent();
            Debug.WriteLine(string.Join(Environment.NewLine, elems.Where(e => e.Path.StartsWith(expandElemPath)).Select(e => e.Path)));
            Debug.Unindent();

            var elem = elems.FirstOrDefault(e => e.Path == expandElemPath);
            testExpandElement(sd, elem);
        }

        void testExpandElement(StructureDefinition sd, ElementDefinition elem)
        {
            Assert.IsNotNull(elem);
            var elems = sd.Snapshot.Element;
            Assert.IsTrue(elems.Contains(elem));

            // Test...
            _generator = new SnapshotGenerator(_testResolver, _settings);
            var result = _generator.ExpandElement(elems, elem);

            // Verify results
            verifyExpandElement(elem, elems, result);
        }

        void verifyExpandElement(ElementDefinition elem, List<ElementDefinition> elems, IList<ElementDefinition> result)
        {
            var expandElemPath = elem.Path;

            // Debug.WriteLine("\r\nOutput:");
            // Debug.WriteLine(string.Join(Environment.NewLine, result.Where(e => e.Path.StartsWith(expandElemPath)).Select(e => e.Path)));

            Assert.IsNotNull(elem.Type);
            var elemType = elem.Type.FirstOrDefault();
            var nameRef = elem.ContentReference;
            if (elemType != null)
            {
                // Validate type profile expansion
                var elemTypeCode = elemType.Code;
                Assert.IsNotNull(elemTypeCode);

                var elemProfile = elemType.Profile;
                var sdType = elemProfile != null && elemTypeCode != FHIRAllTypes.Reference.GetLiteral()
                    ? _testResolver.FindStructureDefinition(elemProfile)
                    : _testResolver.FindStructureDefinitionForCoreType(elemTypeCode);

                // [WMR 20170220] External type profile may not be available
                // Assert.IsNotNull(sdType);
                if (sdType != null)
                {
                    Assert.IsNotNull(sdType.Snapshot);
                    Assert.IsNotNull(sdType.Snapshot.Element);
                    Assert.IsTrue(sdType.Snapshot.Element.Count > 0);

                    // Debug.WriteLine("\r\nType:");
                    // Debug.WriteLine(string.Join(Environment.NewLine, sdType.Snapshot.Element.Select(e => e.Path)));

                    sdType.Snapshot.Rebase(expandElemPath);
                    var typeElems = sdType.Snapshot.Element;

                    var nav = new ElementDefinitionNavigator(result);
                    //Assert.IsTrue(result.Count == elems.Count + typeElems.Count - 1);
                    //if (elem.Name == null)
                    //{
                    //    Assert.IsTrue(result.Where(e => e.Path.StartsWith(expandElemPath)).Count() == typeElems.Count);
                    //}
                    //else
                    if (elem.ContentReference != null)
                    {
                        // Name reference (not a slice)
                        Assert.IsTrue(nav.JumpToNameReference(elem.ContentReference));
                        var cnt = 1;
                        Assert.IsTrue(nav.MoveToFirstChild());
                        do
                        {
                            Assert.AreEqual(typeElems[cnt++].Path, nav.Path);
                        } while (nav.MoveToNext());
                        Assert.AreEqual(typeElems.Count, cnt);
                    }

                    nav.Reset();
                    Assert.IsTrue(nav.MoveTo(elem));
                    Assert.IsTrue(nav.MoveToFirstChild());
                    var typeNav = new ElementDefinitionNavigator(typeElems);
                    Assert.IsTrue(typeNav.MoveTo(typeNav.Elements[0]));
                    Assert.IsTrue(typeNav.MoveToFirstChild());
                    do
                    {
                        var path = typeNav.Path;
                        Assert.IsTrue(nav.Path.EndsWith(path, StringComparison.OrdinalIgnoreCase));
                        if (!nav.MoveToNext())
                        {
                            Debug.Assert(!typeNav.MoveToNext());
                            break;
                        }
                        Debug.Assert(typeNav.MoveToNext());

                    } while (true);
                }


            }
            else if (nameRef != null)
            {
                // Validate name reference expansion
                var nav = new ElementDefinitionNavigator(elems);
                Assert.IsTrue(nav.JumpToNameReference(nameRef));
                var prefix = nav.Path;
                Assert.IsTrue(nav.MoveToFirstChild());
                var pos = result.IndexOf(elem);

                Debug.WriteLine("\r\nName Reference:");
                Debug.Indent();
                do
                {
                    Debug.WriteLine(nav.Path);
                    var srcPath = nav.Path.Substring(prefix.Length);
                    var tgtPath = result[++pos].Path.Substring(expandElemPath.Length);
                    Assert.AreEqual(srcPath, tgtPath);
                } while (nav.MoveToNext());
                Debug.Unindent();
            }
        }

        // [WMR 20160722] For debugging purposes
        [Conditional("DEBUG")]
        void dumpReferences(StructureDefinition sd, bool differential = false)
        {
            if (sd != null)
            {
                Debug.WriteLine("References for StructureDefinition '{0}' ('{1}')".FormatWith(sd.Name, sd.Url));
                Debug.WriteLine("BaseDefinition = '{0}'".FormatWith(sd.BaseDefinition));

                // FhirClient client = new FhirClient("http://fhir2.healthintersections.com.au/open/");
                // var folderPath = Path.Combine(Directory.GetCurrentDirectory(), @"TestData\snapshot-test\download");
                // if (!Directory.Exists(folderPath)) { Directory.CreateDirectory(folderPath); }

                var component = differential ? sd.Differential.Element : sd.Snapshot.Element;
                var profiles = enumerateDistinctTypeProfiles(component);

                Debug.Indent();
                foreach (var profile in profiles)
                {
                    Debug.WriteLine(profile);

                    // How to determine the original filename?
                    //try
                    //{
                    //    var xml = client.Get(profile);
                    //    var filePath = Path.Combine()
                    //    File.WriteAllText(folderPath, )
                    //}
                    //catch (Exception ex)
                    //{
                    //    Debug.WriteLine(ex.Message);
                    //}
                }
                Debug.Unindent();
            }
        }

        static IEnumerable<string> enumerateDistinctTypeProfiles(IList<ElementDefinition> elements)
        {
            return elements.SelectMany(e => e.Type).Select(t => t.Profile).Distinct();
        }

        [Conditional("DEBUG")]
        static void dumpBaseElems(IEnumerable<ElementDefinition> elements)
        {
            Debug.Print(string.Join(Environment.NewLine,
                elements.Select(e =>
                {
                    var bea = e.Annotation<BaseDefAnnotation>();
                    var be = bea != null ? bea.BaseElementDefinition : null;
                    return "  #{0} '{1}' - '{2}' => #{3} '{4}' - '{5}'"
                        .FormatWith(
                            e.GetHashCode(),
                            e.Path,
                            e.Base != null ? e.Base.Path : null,
                            be != null ? (int?)be.GetHashCode() : null,
                            be != null ? be.Path : null,
                            be != null && be.Base != null ? be.Base.Path : null
                        );
                })
            ));
        }

        [Conditional("DEBUG")]
        void dumpBasePaths(StructureDefinition sd)
        {
            if (sd != null && sd.Snapshot != null)
            {
                Debug.WriteLine("StructureDefinition '{0}' ('{1}')".FormatWith(sd.Name, sd.Url));
                Debug.WriteLine("BaseDefiniton = '{0}'".FormatWith(sd.BaseDefinition));
                // Debug.Indent();
                Debug.Print("Element.Id | Element.Path | Element.Base.Path");
                Debug.Print(new string('=', 100));
                foreach (var elem in sd.Snapshot.Element)
                {
                    Debug.WriteLine("{0}  |  {1}  |  {2}", elem.ElementId, elem.Path, elem.Base?.Path);
                }
                // Debug.Unindent();
            }
        }

        [Conditional("DEBUG")]
        void dumpOutcome(OperationOutcome outcome)
        {
            if (outcome != null)
            {
                Debug.Print("===== OperationOutcome: {0} issues", outcome.Issue.Count);
                for (int i = 0; i < outcome.Issue.Count; i++)
                {
                    dumpIssue(outcome.Issue[i], i);
                }
                Debug.Print("==================================");
            }
        }

        [Conditional("DEBUG")]
        private void dumpIssue(OperationOutcome.IssueComponent issue, int index)
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendFormat("* Issue #{0}: Severity = '{1}' Code = '{2}'", index, issue.Severity, issue.Code);
            if (issue.Details != null)
            {
                sb.AppendFormat(" Details: '{0}'", string.Join(" | ", issue.Details.Coding.Select(c => c.Code)));
                if (issue.Details.Text != null) sb.AppendFormat(" Text : '{0}'", issue.Details.Text);
            }
            if (issue.Diagnostics != null) { sb.AppendFormat(" Profile: '{0}'", issue.Diagnostics); }
            if (issue.Location != null) { sb.AppendFormat(" Path: '{0}'", string.Join(" | ", issue.Location)); }

            Debug.Print(sb.ToString());
        }


        [TestMethod]
        public void GenerateSnapshotEmitBaseData()
        {
            // Verify that the SnapshotGenerator events provide stable references to associated base ElementDefinition instances.
            // If two different profile elements have the same type, then the PrepareElement event should provide the exact same
            // reference to the associated base element. The same target ElementDefinition instance should also be contained in
            // the external type profile.

            var source = _testResolver;
            Assert.IsNotNull(source);

            // var sd = source.FindStructureDefinition(@"http://hl7.org/fhir/StructureDefinition/daf-condition");
            // var sd = source.FindStructureDefinition(@"http://example.com/fhir/StructureDefinition/patient-with-extensions");
            // var sd = source.FindStructureDefinition(@"http://hl7.org/fhir/StructureDefinition/sdc-questionnaire");
            // var sd = source.FindStructureDefinition(@"http://hl7.org/fhir/StructureDefinition/cqif-guidanceartifact");
            // var sd = source.FindStructureDefinition(@"http://hl7.org/fhir/StructureDefinition/shareablevalueset");
            // var sd = source.FindStructureDefinition(@"http://hl7.org/fhir/StructureDefinition/qicore-goal");
            // var sd = source.FindStructureDefinition(@"http://hl7.org/fhir/StructureDefinition/cqif-guidanceartifact");
            // var sd = source.FindStructureDefinition(@"http://example.org/fhir/StructureDefinition/MyLocation");
            // var sd = source.FindStructureDefinition(@"http://example.org/fhir/StructureDefinition/MyPatient");
            // var sd = source.FindStructureDefinition(@"http://example.org/fhir/StructureDefinition/MyExtension1");
            // var sd = source.FindStructureDefinition(@"http://hl7.org/fhir/StructureDefinition/CarePlan");

            // var sd = source.FindStructureDefinition(@"http://hl7.org/fhir/StructureDefinition/Element");
            // var sd = source.FindStructureDefinition(@"http://hl7.org/fhir/StructureDefinition/Patient");
            // var sd = source.FindStructureDefinition(@"http://hl7.org/fhir/StructureDefinition/Extension");
            // var sd = source.FindStructureDefinition(@"http://hl7.org/fhir/StructureDefinition/Meta");
            // var sd = source.FindStructureDefinition(@"http://hl7.org/fhir/StructureDefinition/Money");

            // var sd = source.FindStructureDefinition(@"http://hl7.org/fhir/StructureDefinition/cqif-basic-guidance-action");

            // var sd = source.FindStructureDefinition(@"http://example.org/fhir/StructureDefinition/PatientWithExtension");
            // var sd = source.FindStructureDefinition(@"http://example.org/fhir/StructureDefinition/PatientWithCustomIdentifier");

            var sd = source.FindStructureDefinition(@"http://example.org/fhir/StructureDefinition/CustomIdentifier");

            Assert.IsNotNull(sd);
            // dumpReferences(sd);

            var settings = new SnapshotGeneratorSettings(_settings);
            // settings.GenerateExtensionsOnConstraints = true;
            settings.GenerateAnnotationsOnConstraints = true;
            _generator = new SnapshotGenerator(source, settings);

            try
            {
                _generator.PrepareBaseProfile += profileHandler;
                _generator.PrepareElement += elementHandler;
                _generator.Constraint += constraintHandler;

                StructureDefinition expanded;
                generateSnapshotAndCompare(sd, out expanded);

                dumpOutcome(_generator.Outcome);

                assertBaseDefs(expanded, settings);

                if (sd.Url != ModelInfo.CanonicalUriForFhirCoreType(FHIRAllTypes.Element))
                {
                    // Element snapshot should be recursively expanded, as it is the fundamental base profile
                    var sdElem = source.FindStructureDefinitionForCoreType(FHIRAllTypes.Element);
                    Assert.IsNotNull(sdElem);
                    Assert.IsTrue(sdElem.HasSnapshot);
                    Assert.IsTrue(sdElem.Snapshot.IsCreatedBySnapshotGenerator());
                    assertBaseDefs(sdElem, settings);
                }

                if (sd.Url != ModelInfo.CanonicalUriForFhirCoreType(FHIRAllTypes.Id))
                {
                    // Id snapshot should not be (re-)generated, as derived profiles don't force expansion
                    var sdId = source.FindStructureDefinitionForCoreType(FHIRAllTypes.Id);
                    Assert.IsNotNull(sdId);
                    Assert.IsTrue(sdId.HasSnapshot);
                    Assert.IsFalse(sdId.Snapshot.IsCreatedBySnapshotGenerator());
                    // Re-generate the snapshot and verify base references
                    generateSnapshotAndCompare(sdId, out expanded);
                    assertBaseDefs(expanded, settings);
                }

                if (sd.Url == @"http://example.org/fhir/StructureDefinition/MyPatient")
                {
                    var sdBase = source.FindStructureDefinition(@"http://hl7.org/fhir/StructureDefinition/Patient");
                    assertBaseDefs(sdBase, settings);

                    var sdElem = source.FindStructureDefinition(@"http://hl7.org/fhir/StructureDefinition/Element");
                    assertBaseDefs(sdElem, settings);

                    var sdExt = source.FindStructureDefinition(@"http://hl7.org/fhir/StructureDefinition/Extension");
                    assertBaseDefs(sdExt, settings);

                    var sdExt1 = source.FindStructureDefinition(@"http://example.org/fhir/StructureDefinition/MyExtension1");
                    assertBaseDefs(sdExt1, settings);

                    var sdExt2 = source.FindStructureDefinition(@"http://example.org/fhir/StructureDefinition/MyExtension2");
                    assertBaseDefs(sdExt2, settings);
                }

            }
            finally
            {
                // Detach event handlers
                _generator.Constraint -= constraintHandler;
                _generator.PrepareElement -= elementHandler;
                _generator.PrepareBaseProfile -= profileHandler;
            }
        }

        [TestMethod]
        public void TestBaseAnnotations_ExplicitCoreTypeProfile()
        {
            // Verify processing of explicit core element type profile in differential
            // e.g. if the differential specifies explicit core type profile url
            // Example: Patient.identifier type = { Code : Identifier, Profile : "http://hl7.org/fhir/StructureDefinition/Identifier" } }
            // Snapshot generator should ignore this, i.e. NOT treat this as a constraint

            var source = _testResolver;
            Assert.IsNotNull(source);
            var sd = source.FindStructureDefinition(@"http://example.org/fhir/StructureDefinition/PatientWithExplicitCoreIdentifierProfile");

            Assert.IsNotNull(sd);
            // dumpReferences(sd);

            // Patient.identifier should reference the default core Identifier type profile
            var elem = sd.Differential.Element.FirstOrDefault(e => e.Path == "Patient.identifier");
            Assert.IsNotNull(elem);
            var typeProfileUrl = elem.Type.FirstOrDefault().Profile;
            Assert.IsNotNull(typeProfileUrl);
            Assert.AreEqual(typeProfileUrl, ModelInfo.CanonicalUriForFhirCoreType(FHIRAllTypes.Identifier));

            var settings = new SnapshotGeneratorSettings(_settings);
            settings.GenerateAnnotationsOnConstraints = true;
            _generator = new SnapshotGenerator(source, settings);

            try
            {
                _generator.PrepareBaseProfile += profileHandler;
                _generator.PrepareElement += elementHandler;
                _generator.Constraint += constraintHandler;

                StructureDefinition expanded;
                generateSnapshotAndCompare(sd, out expanded);
                dumpOutcome(_generator.Outcome);
                Assert.IsTrue(expanded.HasSnapshot);
                Assert.IsTrue(expanded.Snapshot.IsCreatedBySnapshotGenerator());
                assertBaseDefs(expanded, settings);

                // Verify that the snapshot generator also expanded the referenced core Identifier type profile
                var sdType = source.FindStructureDefinitionForCoreType(FHIRAllTypes.Identifier);
                Assert.IsNotNull(sdType);
                Assert.IsTrue(sdType.HasSnapshot);
                Assert.IsTrue(sdType.Snapshot.IsCreatedBySnapshotGenerator());

                // Verify the snapshot expansion of the Patient.identifier element
                elem = expanded.Snapshot.Element.FirstOrDefault(e => e.Path == "Patient.identifier");
                Assert.IsNotNull(elem);
                var baseElem = elem.Annotation<BaseDefAnnotation>()?.BaseElementDefinition;
                Assert.IsNotNull(baseElem);
                Assert.AreEqual(elem.Path, baseElem.Path); // Base = core Patient.identifier element
                // Note: diff elem is not exactly equal to base elem (due to reduntant type profile constraint)
                // hasConstraints and hasChanges methods aren't smart enough to detect redundant constraints
                var hasConstraints = SnapshotGeneratorTest2.hasConstraints(elem, baseElem);
                Assert.IsTrue(hasConstraints);      
                Assert.IsTrue(hasChanges(elem));

                // Verify base annotations on Patient.identifier subtree
                var elems = expanded.Snapshot.Element.Where(e => e.Path.StartsWith("Patient.identifier.")).ToList();
                for (int i = 0; i < elems.Count; i++)
                {
                    elem = elems[i];
                    Assert.IsNotNull(elem);
                    baseElem = elem.Annotation<BaseDefAnnotation>()?.BaseElementDefinition;
                    Assert.IsNotNull(baseElem);
                    hasConstraints = SnapshotGeneratorTest2.hasConstraints(elem, baseElem);
                    // Only the .use child element has a profile diff constraint
                    bool isConstrained = elem.Path == "Patient.identifier.use";
                    Assert.AreEqual(isConstrained, hasConstraints);
                    Assert.AreEqual(isConstrained, hasChanges(elem));

                    // Verify that base element annotations reference the associated child element in Core Identifier profile
                    Assert.AreEqual("Patient." + baseElem.Path.Uncapitalize(), elem.Path);
                }

            }
            finally
            {
                // Detach event handlers
                _generator.Constraint -= constraintHandler;
                _generator.PrepareElement -= elementHandler;
                _generator.PrepareBaseProfile -= profileHandler;
            }
        }

        [TestMethod]
        public void TestBaseAnnotations_CustomTypeProfile()
        {
            // Verify generated base annotations for a profile that references an external element type profile
            // e.g. Patient profile with a custom Identifier profile on the Patient.identifier element

            var source = _testResolver;
            Assert.IsNotNull(source);
            var sd = source.FindStructureDefinition(@"http://example.org/fhir/StructureDefinition/PatientWithCustomIdentifier");

            Assert.IsNotNull(sd);
            // dumpReferences(sd);

            // Patient.identifier should reference an external type profile
            var elem = sd.Differential.Element.FirstOrDefault(e => e.Path == "Patient.identifier");
            Assert.IsNotNull(elem);
            var typeProfileUrl = elem.Type.FirstOrDefault().Profile;
            Assert.IsNotNull(typeProfileUrl);

            var settings = new SnapshotGeneratorSettings(_settings);
            settings.GenerateAnnotationsOnConstraints = true;
            _generator = new SnapshotGenerator(source, settings);

            try
            {
                _generator.PrepareBaseProfile += profileHandler;
                _generator.PrepareElement += elementHandler;
                _generator.Constraint += constraintHandler;

                StructureDefinition expanded;
                generateSnapshotAndCompare(sd, out expanded);
                dumpOutcome(_generator.Outcome);
                Assert.IsTrue(expanded.HasSnapshot);
                Assert.IsTrue(expanded.Snapshot.IsCreatedBySnapshotGenerator());
                assertBaseDefs(expanded, settings);

                // Verify that the snapshot generator also expanded the referenced external custom Identifier type profile
                var sdType = source.FindStructureDefinition(typeProfileUrl);
                Assert.IsNotNull(sdType);
                Assert.IsTrue(sdType.HasSnapshot);
                Assert.IsTrue(sdType.Snapshot.IsCreatedBySnapshotGenerator());
                assertBaseDefs(sdType, settings);

                // Verify the snapshot expansion of the Patient.identifier element
                elem = expanded.Snapshot.Element.FirstOrDefault(e => e.Path == "Patient.identifier");
                Assert.IsNotNull(elem);
                var baseElem = elem.Annotation<BaseDefAnnotation>()?.BaseElementDefinition;
                Assert.IsNotNull(baseElem);
                Assert.AreEqual(elem.Path, baseElem.Path); // Base = core Patient.identifier element
                // Note: diff elem is not exactly equal to base elem (due to reduntant type profile constraint)
                // hasConstraints and hasChanges methods aren't smart enough to detect redundant constraints
                var hasConstraints = SnapshotGeneratorTest2.hasConstraints(elem, baseElem);
                Assert.IsTrue(hasConstraints);

                // Verify base annotations on Patient.identifier subtree
                var elems = expanded.Snapshot.Element.Where(e => e.Path.StartsWith("Patient.identifier.")).ToList();
                for (int i = 0; i < elems.Count; i++)
                {
                    elem = elems[i];
                    Assert.IsNotNull(elem);
                    baseElem = elem.Annotation<BaseDefAnnotation>()?.BaseElementDefinition;
                    Assert.IsNotNull(baseElem);
                    hasConstraints = SnapshotGeneratorTest2.hasConstraints(elem, baseElem);
                    // Only the .use child element has a profile diff constraint
                    bool isConstrained = elem.Path == "Patient.identifier.use" || elem.Path == "Patient.identifier.value";
                    Assert.AreEqual(isConstrained, hasConstraints);
                    Assert.AreEqual(isConstrained, hasChanges(elem));

                    // Verify that base element annotations reference the associated child element in custom Identifier profile
                    // Assert.AreEqual("Patient." + baseElem.Path.Uncapitalize(), elem.Path);

                    // Verify correct base element annotations
                    // Should point to rebased custom type element (same path)
                    Assert.AreEqual(baseElem.Path, elem.Path);
                }

                // Verify specific element constraints
                // Patient.identifier.use::min is overriden by patient profile
                elem = elems.FirstOrDefault(e => e.Path == "Patient.identifier.use");
                Assert.IsNotNull(elem);
                Assert.AreEqual(1, elem.Min);
                Assert.IsTrue(elem.HasDiffConstraintAnnotations());
                Assert.IsTrue(elem.MinElement.IsConstrainedByDiff());

                // Patient.identifier.value::short is overriden by patient profile
                elem = elems.FirstOrDefault(e => e.Path == "Patient.identifier.value");
                Assert.IsNotNull(elem);
                Assert.AreEqual("A custom identifier value", elem.Short);
                Assert.IsTrue(elem.HasDiffConstraintAnnotations());
                Assert.IsTrue(elem.ShortElement.IsConstrainedByDiff());

                // Patient.identifier.system::min is inherited from custom type profile, not overriden by patient profile
                elem = elems.FirstOrDefault(e => e.Path == "Patient.identifier.system");
                Assert.IsNotNull(elem);
                Assert.AreEqual(1, elem.Min);
                Assert.IsFalse(elem.HasDiffConstraintAnnotations());
                Assert.IsFalse(elem.MinElement.IsConstrainedByDiff());

            }
            finally
            {
                // Detach event handlers
                _generator.Constraint -= constraintHandler;
                _generator.PrepareElement -= elementHandler;
                _generator.PrepareBaseProfile -= profileHandler;
            }
        }

        [TestMethod]
        public void TestBaseAnnotations_InlineExtension()
        {
            // Verify generated base annotations for a profile that references an external extension definition profile

            var source = _testResolver;
            Assert.IsNotNull(source);
            var sd = source.FindStructureDefinition(@"http://example.org/fhir/StructureDefinition/PatientWithExtension");

            Assert.IsNotNull(sd);
            // dumpReferences(sd);

            // Patient profile should reference an external extension definition, fetch the url
            var elem = sd.Differential.Element.FirstOrDefault(e => e.Path == "Patient.extension" && e.Slicing == null);
            Assert.IsNotNull(elem);
            var extensionDefinitionUrl = elem.Type.FirstOrDefault().Profile;
            Assert.IsNotNull(extensionDefinitionUrl);

            var settings = new SnapshotGeneratorSettings(_settings);
            settings.GenerateAnnotationsOnConstraints = true;
            _generator = new SnapshotGenerator(source, settings);

            try
            {
                _generator.PrepareBaseProfile += profileHandler;
                _generator.PrepareElement += elementHandler;
                _generator.Constraint += constraintHandler;

                StructureDefinition expanded;
                generateSnapshotAndCompare(sd, out expanded);
                dumpOutcome(_generator.Outcome);
                Assert.IsTrue(expanded.HasSnapshot);
                Assert.IsTrue(expanded.Snapshot.IsCreatedBySnapshotGenerator());
                assertBaseDefs(expanded, settings);

                // Verify that the snapshot generator also expanded the referenced external extension definition
                var sdExtension = source.FindStructureDefinition(extensionDefinitionUrl);
                Assert.IsNotNull(sdExtension);
                Assert.IsTrue(sdExtension.HasSnapshot);
                Assert.IsTrue(sdExtension.Snapshot.IsCreatedBySnapshotGenerator());
                assertBaseDefs(sdExtension, settings);

                // Verify correct merging of inline profile constraints overriding the extension definition
                var nav = new ElementDefinitionNavigator(expanded);
                Assert.IsTrue(nav.MoveToFirstChild());
                Assert.IsTrue(nav.MoveToFirstChild());
                Assert.IsTrue(nav.MoveToNext("extension"));
                Assert.IsNotNull(nav.Current.Slicing);  // Extension slicing entry
                Assert.IsTrue(nav.MoveToNext("extension"));
                elem = nav.Current;
                Assert.IsNull(elem.Slicing);    // First extension
                Assert.AreEqual(elem.PrimaryTypeProfile(), extensionDefinitionUrl);

                Assert.AreEqual("extension", elem.SliceName);
                Assert.AreEqual("1", elem.Max); // Inline profile constraint overriding the extension definition
                Assert.IsTrue(elem.MaxElement.IsConstrainedByDiff());
                Assert.IsTrue(elem.HasDiffConstraintAnnotations());
                Assert.IsTrue(elem.IsConstrainedByDiff());
                var baseElem = elem.Annotation<BaseDefAnnotation>()?.BaseElementDefinition;
                Assert.IsNotNull(baseElem);
                Assert.AreEqual("*", baseElem.Max);             // Verify that max property is not inherited from base element = Extension root element
                Assert.AreEqual(baseElem.Short, elem.Short);    // Verify that short property is inherited
                Assert.IsFalse(elem.ShortElement.IsConstrainedByDiff());
                // Profile overrides the definition property of the extension definition root element 
                Assert.AreNotEqual(baseElem.Definition, elem.Definition);
                Assert.IsTrue(elem.DefinitionElement.IsConstrainedByDiff());

                Assert.IsTrue(nav.MoveToFirstChild());

                Assert.IsTrue(nav.MoveToNext("url"));
                elem = nav.Current;
                Assert.IsFalse(elem.HasDiffConstraintAnnotations());
                var uri = elem.Fixed as FhirUri;
                Assert.IsNotNull(uri);
                Assert.AreEqual(extensionDefinitionUrl, uri.Value);

                Assert.IsTrue(nav.MoveToNext("valueString"));
                elem = nav.Current;
                Assert.AreEqual(1, elem.Min);            // Inline profile constraint overriding the extension definition
                Assert.IsTrue(elem.MinElement.IsConstrainedByDiff());
                Assert.IsTrue(elem.HasDiffConstraintAnnotations());
                baseElem = elem.Annotation<BaseDefAnnotation>()?.BaseElementDefinition;
                Assert.IsNotNull(baseElem);
                Assert.AreEqual(0, baseElem.Min);               // Verify that min property is not inherited from base element = Extension.valueString
                Assert.AreEqual(baseElem.Short, elem.Short);    // Verify that short property is inherited
                Assert.IsFalse(elem.ShortElement.IsConstrainedByDiff());
                Assert.AreEqual(baseElem.Definition, elem.Definition);    // Verify that definition property is inherited
                Assert.IsFalse(elem.DefinitionElement.IsConstrainedByDiff());
            }
            finally
            {
                // Detach event handlers
                _generator.Constraint -= constraintHandler;
                _generator.PrepareElement -= elementHandler;
                _generator.PrepareBaseProfile -= profileHandler;
            }
        }


        // [WMR 20160816] Test custom annotations containing associated base definitions
        class BaseDefAnnotation
        {
            public BaseDefAnnotation(ElementDefinition baseElemDef) { BaseElementDefinition = baseElemDef; }
            public ElementDefinition BaseElementDefinition { get; private set; }
        }

        static ElementDefinition GetBaseElementAnnotation(ElementDefinition elemDef)
        {
            return elemDef?.Annotation<BaseDefAnnotation>()?.BaseElementDefinition;
        }

        void profileHandler(object sender, SnapshotBaseProfileEventArgs e)
        {
            var profile = e.Profile;
            // Assert.IsTrue(sd.Url != profile.Url || sd.IsExactly(profile));
            var baseProfile = e.BaseProfile;
            Assert.IsNotNull(baseProfile);
            Debug.WriteLine("[SnapshotBaseProfileHandler] Profile #{0} '{1}' BaseDefinition = '{2}'".FormatWith(profile.GetHashCode(), profile.Url, profile.BaseDefinition));
            Debug.Print("[SnapshotBaseProfileHandler] Base Profile #{0} '{1}'".FormatWith(baseProfile.GetHashCode(), baseProfile.Url));
            var rootElem = baseProfile.Snapshot.Element[0];
            Debug.Print("[SnapshotBaseProfileHandler] Base Root element #{0} '{1}'".FormatWith(rootElem.GetHashCode(), rootElem.Path));
            Assert.AreEqual(profile.BaseDefinition, baseProfile.Url);
        }

        void elementHandler(object sender, SnapshotElementEventArgs e)
        {
            var elem = e.Element;
            Assert.IsNotNull(elem);

            // Assert.IsNotNull(elem.Base);

            var ann = elem.Annotation<BaseDefAnnotation>();
            // We want to annotate a reference to the matching base element from the (immediate) base profile.
            // When the snapshot generator expands external profiles, then this handler is called once for each
            // profile in the base hierarchy, starting at the root profile, e.g. Resource => DomainResource => Patient.
            // Each time we recreate the annotation, so the final annotation contains a reference to the immediate base.
            if (ann != null)
            {
                elem.RemoveAnnotations<BaseDefAnnotation>();
            }
            var baseDef = e.BaseElement;
            var baseStruct = e.BaseStructure;
            elem.AddAnnotation(new BaseDefAnnotation(baseDef));
            Debug.Write("[SnapshotElementHandler] #{0} '{1}' - Base: #{2} '{3}' - Base Structure '{4}'".FormatWith(elem.GetHashCode(), elem.Path, baseDef != null ? baseDef.GetHashCode() : 0, baseDef != null ? baseDef.Path : null, baseStruct != null ? baseStruct.Url : null));
            Debug.WriteLine(ann != null && ann.BaseElementDefinition != null ? " (old Base: #{0} '{1}')".FormatWith(ann.BaseElementDefinition.GetHashCode(), ann.BaseElementDefinition.Path) : "");
        }

        void constraintHandler(object sender, SnapshotConstraintEventArgs e)
        {
            var elem = e.Element as ElementDefinition;
            if (elem != null)
            {
                // var changed = elem.GetChangedByDiff() == true;
                var changed = elem.IsConstrainedByDiff();
                Debug.Assert(!_settings.GenerateAnnotationsOnConstraints || changed);
                Debug.Print("[SnapshotConstraintHandler] #{0} '{1}'{2}".FormatWith(elem.GetHashCode(), elem.Path, changed ? " CHANGED!" : null));
            }
        }

        static void assertBaseDefs(StructureDefinition sd, SnapshotGeneratorSettings settings)
        {
            Assert.IsNotNull(sd);
            Assert.IsNotNull(sd.Snapshot);
            var elems = sd.Snapshot.Element;
            Assert.IsNotNull(elems);
            Assert.IsTrue(elems.Count > 0);

            var isConstraint = sd.Derivation == StructureDefinition.TypeDerivationRule.Constraint;

            Debug.Print("\r\nStructureDefinition '{0}' url = '{1}'", sd.Name, sd.Url);
            Debug.Print("# | Constraints? | Changed? | Element.Path | Element.Base.Path | BaseElement.Path | #Base | Invalid?");
            Debug.Print(new string('=', 100));
            foreach (var elem in elems)
            {
                // Each element should have a valid Base component, unless the profile is a core type/resource definition (no base)
                // Assert.IsTrue(!isConstraint || elem.Base != null);

                var ann = elem.Annotation<BaseDefAnnotation>();
                var baseDef = ann != null ? ann.BaseElementDefinition : null;
                Assert.AreNotEqual(elem, baseDef);

                var hasChanges = SnapshotGeneratorTest2.hasChanges(elem);
                var hasConstraints = false;
                if (baseDef != null) // && elem.Base != null)
                {
                    // If normalizing, then elem.Base.Path refers to the defining profile (e.g. DomainResource),
                    // whereas baseDef refers to the immediate base profile (e.g. Patient)
                    Debug.Assert(elem.Base == null || ElementDefinitionNavigator.IsCandidateBasePath(elem.Base.Path, baseDef.Path));
                    hasConstraints = SnapshotGeneratorTest2.hasConstraints(elem, baseDef);
                }
                var isValid = hasChanges == hasConstraints;
                bool? hasConstraintAnnotations = null;
                if (settings.GenerateAnnotationsOnConstraints)
                {
                    hasConstraintAnnotations = elem.HasDiffConstraintAnnotations();
                    isValid &= hasConstraints == hasConstraintAnnotations;
                }

                Debug.WriteLine("{0,10}  |  {1}  |  {2,-12}  |  {3,-50}  |  {4,-40}  |  {5,-40}  |  {6,10}  |  {7}",
                    elem.GetHashCode(),
                    (hasConstraints ? "+" : "-")
                    + (hasConstraintAnnotations.HasValue ? (hasConstraintAnnotations.Value ? " (+)" : " (-)") : null),
                    getChangeDescription(elem),
                    elem.Path,
                    elem.Base != null ? elem.Base.Path : null,
                    baseDef != null ? baseDef.Path : null,
                    baseDef != null ? baseDef.GetHashCode().ToString() : null,
                    !isValid ? "!!!" : ""
                );
                //Assert.IsTrue(baseDef == null || isValid);
                // Debug.Assert(baseDef == null || isValid);
            }
        }

        // Utility function to compare element and base element
        // Path, Base and CHANGED_BY_DIFF_EXT extension are excluded from comparison
        // Returns true if the element has any other constraints on base
        static bool hasConstraints(ElementDefinition elem, ElementDefinition baseElem)
        {
            var elemClone = (ElementDefinition)elem.DeepCopy();
            var baseClone = (ElementDefinition)baseElem.DeepCopy();

            // Id, Path & Base are expected to differ
            baseClone.ElementId = elem.ElementId;
            baseClone.Path = elem.Path;
            baseClone.Base = elem.Base;

            // Also ignore any Changed extensions on base and diff
            elemClone.RemoveAllConstrainedByDiffExtensions();
            baseClone.RemoveAllConstrainedByDiffExtensions();
            elemClone.RemoveAllConstrainedByDiffAnnotations();
            baseClone.RemoveAllConstrainedByDiffAnnotations();

            var result = !baseClone.IsExactly(elemClone);
            return result;
        }

        // Returns true if the specified element or any of its' components contain the CHANGED_BY_DIFF_EXT extension
        static bool hasChanges(ElementDefinition elem)
        {
            return isChanged(elem)
                || hasChanges(elem.AliasElement)
                || isChanged(elem.Base)
                || isChanged(elem.Binding)
                || hasChanges(elem.Code)
                || isChanged(elem.CommentElement)
                || hasChanges(elem.ConditionElement)
                || hasChanges(elem.Constraint)
                || isChanged(elem.DefaultValue)
                || isChanged(elem.DefinitionElement)
                || hasChanges(elem.Example)
                || hasChanges(elem.Extension)
                || hasChanges(elem.FhirCommentsElement)
                || isChanged(elem.Fixed)
                || isChanged(elem.IsModifierElement)
                || isChanged(elem.IsSummaryElement)
                || isChanged(elem.LabelElement)
                || hasChanges(elem.Mapping)
                || isChanged(elem.MaxElement)
                || isChanged(elem.MaxLengthElement)
                || isChanged(elem.MaxValue)
                || isChanged(elem.MeaningWhenMissingElement)
                || isChanged(elem.MinElement)
                || isChanged(elem.MinValue)
                || isChanged(elem.MustSupportElement)
                || isChanged(elem.SliceNameElement)
                || isChanged(elem.ContentReferenceElement)
                || isChanged(elem.PathElement)
                || isChanged(elem.Pattern)
                || hasChanges(elem.RepresentationElement)
                || isChanged(elem.RequirementsElement)
                || isChanged(elem.ShortElement)
                || isChanged(elem.Slicing)
                || hasChanges(elem.Type);
        }

        static string getChangeDescription(ElementDefinition element)
        {
            if (isChanged(element.Slicing)) { return "Slicing"; }       // Moved to front
            if (hasChanges(element.Type)) { return "Type"; }            // Moved to front
            if (isChanged(element.ShortElement)) { return "Short"; }    // Moved to front

            if (hasChanges(element.AliasElement)) { return "Alias"; }
            if (isChanged(element.Base)) { return "Base"; }
            if (isChanged(element.Binding)) { return "Binding"; }
            if (hasChanges(element.Code)) { return "Code"; }
            if (isChanged(element.CommentElement)) { return "Comment"; }
            if (hasChanges(element.ConditionElement)) { return "Condition"; }
            if (hasChanges(element.Constraint)) { return "Constraint"; }
            if (isChanged(element.DefaultValue)) { return "DefaultValue"; }
            if (isChanged(element.DefinitionElement)) { return "Definition"; }
            if (hasChanges(element.Example)) { return "Example"; }
            if (hasChanges(element.Extension)) { return "Extension"; }
            if (hasChanges(element.FhirCommentsElement)) { return "FhirComments"; }
            if (isChanged(element.Fixed)) { return "Fixed"; }
            if (isChanged(element.IsModifierElement)) { return "IsModifier"; }
            if (isChanged(element.IsSummaryElement)) { return "IsSummary"; }
            if (isChanged(element.LabelElement)) { return "Label"; }
            if (hasChanges(element.Mapping)) { return "Mapping"; }
            if (isChanged(element.MaxElement)) { return "Max"; }
            if (isChanged(element.MaxLengthElement)) { return "MaxLength"; }
            if (isChanged(element.MaxValue)) { return "MaxValue"; }
            if (isChanged(element.MeaningWhenMissingElement)) { return "MeaningWhenMissing"; }
            if (isChanged(element.MinElement)) { return "Min"; }
            if (isChanged(element.MinValue)) { return "MinValue"; }
            if (isChanged(element.MustSupportElement)) { return "MustSupport"; }
            if (isChanged(element.SliceNameElement)) { return "SliceName"; }
            if (isChanged(element.ContentReferenceElement)) { return "ContentReference"; }
            if (isChanged(element.PathElement)) { return "Path"; }
            if (isChanged(element.Pattern)) { return "Pattern"; }
            if (hasChanges(element.RepresentationElement)) { return "Representation"; }
            if (isChanged(element.RequirementsElement)) { return "Requirements"; }
            //if (IsChanged(element.ShortElement)) { return "Short"; }
            //if (IsChanged(element.Slicing)) { return "Slicing"; }
            //if (HasChanges(element.Type)) { return "Type"; }

            if (isChanged(element)) { return "Element"; }           // Moved to back

            return string.Empty;
        }

        // static bool hasChanges<T>(IList<T> extendables) where T : IExtendable => extendables != null ? extendables.Any(e => isChanged(e)) : false;

        // static bool isChanged(IExtendable extendable) => extendable != null && extendable.GetChangedByDiff() == true;

        static bool hasChanges<T>(IList<T> elements) where T : Element => elements != null ? elements.Any(e => isChanged(e)) : false;
        static bool isChanged(Element elem) => elem != null && elem.IsConstrainedByDiff();

        [TestMethod]
        public void TestExpandCoreArtifacts()
        {
            // testExpandResource(@"http://hl7.org/fhir/StructureDefinition/Element");
            // testExpandResource(@"http://hl7.org/fhir/StructureDefinition/BackboneElement");
            testExpandResource(@"http://hl7.org/fhir/StructureDefinition/Extension");

            //testExpandResource(@"http://hl7.org/fhir/StructureDefinition/integer");
            //testExpandResource(@"http://hl7.org/fhir/StructureDefinition/positiveInt");
            //testExpandResource(@"http://hl7.org/fhir/StructureDefinition/string");
            //testExpandResource(@"http://hl7.org/fhir/StructureDefinition/code");
            //testExpandResource(@"http://hl7.org/fhir/StructureDefinition/id");

            //testExpandResource(@"http://hl7.org/fhir/StructureDefinition/Meta");
            //testExpandResource(@"http://hl7.org/fhir/StructureDefinition/HumanName");
            //testExpandResource(@"http://hl7.org/fhir/StructureDefinition/Quantity");
            //testExpandResource(@"http://hl7.org/fhir/StructureDefinition/SimpleQuantity");
            //testExpandResource(@"http://hl7.org/fhir/StructureDefinition/Money");

            // testExpandResource(@"http://hl7.org/fhir/StructureDefinition/Resource");
            // testExpandResource(@"http://hl7.org/fhir/StructureDefinition/DomainResource");

            //testExpandResource(@"http://hl7.org/fhir/StructureDefinition/Basic");
            //testExpandResource(@"http://hl7.org/fhir/StructureDefinition/Patient");
            //testExpandResource(@"http://hl7.org/fhir/StructureDefinition/Questionnaire");
            //testExpandResource(@"http://hl7.org/fhir/StructureDefinition/AuditEvent");

        }

        [TestMethod]
        public void TestExpandAllCoreTypes()
        {
            // Generate snapshots for all core types, in the original order as they are defined
            // The Snapshot Generator should recursively process any referenced base/type profiles (e.g. Element, Extension)
            var coreArtifactNames = ModelInfo.FhirCsTypeToString.Values;
            var coreTypeUrls = coreArtifactNames.Where(t => !ModelInfo.IsKnownResource(t)).Select(t => "http://hl7.org/fhir/StructureDefinition/" + t).ToArray();
            testExpandResources(coreTypeUrls.ToArray());
        }

        [TestMethod]
        public void TestExpandAllCoreResources()
        {
            // Generate snapshots for all core resources, in the original order as they are defined
            // The Snapshot Generator should recursively process any referenced base/type profiles (e.g. data types)
            var coreResourceUrls = ModelInfo.SupportedResources.Select(t => "http://hl7.org/fhir/StructureDefinition/" + t);
            testExpandResources(coreResourceUrls.ToArray());
        }

        void testExpandResources(string[] profileUris)
        {
            var sw = new Stopwatch();
            int count = profileUris.Length;
            _source.Reset();
            sw.Start();

            for (int i = 0; i < count; i++)
            {
                testExpandResource(profileUris[i]);
            }

            sw.Stop();
            _source.ShowDuration(count, sw.Elapsed);
        }

        bool testExpandResource(string url)
        {
            Debug.Print("[testExpandResource] url = '{0}'", url);
            var sd = _testResolver.FindStructureDefinition(url);
            Assert.IsNotNull(sd);
            // dumpReferences(sd);

            StructureDefinition expanded;
            var result = generateSnapshotAndCompare(sd, out expanded);

            dumpOutcome(_generator.Outcome);
            dumpBasePaths(expanded);

            if (!result)
            {
                Debug.Print("Expanded is not exactly equal to original... verifying...");
                result = verifyElementBase(sd, expanded);
            }

            // Core artifact snapshots are incorrect, e.g. url snapshot is missing extension element
            //Assert.IsTrue(result);

            return result;
        }

        IEnumerable<T> enumerateBundleStream<T>(Stream stream) where T : Resource
        {
            using (var reader = XmlReader.Create(stream))
            {
                var parser = new FhirXmlParser();
                var bundle = parser.Parse<Bundle>(reader);
                foreach (var entry in bundle.Entry)
                {
                    var res = entry.Resource as T;
                    if (res != null) { yield return res; }
                }
            }
        }

        [TestMethod]
        public void TestExpandCoreTypesByHierarchy()
        {
            // [WMR 20160912] Expand all core data types
            // Start at root types without a base (Element, Extension), then recursively expand derived types

            var result = true;
            var source = new DirectorySource("TestData/snapshot-test", false);
            var resolver = new CachedResolver(source); // IMPORTANT!

            _generator = new SnapshotGenerator(resolver, _settings);
            _generator.PrepareElement += elementHandler;

            try
            {
                // HACK! CachedResolver doesn't expose LoadArtifactByName
                // So first enumerate source to get url's, then enumerate CachedResolver to persist snapshots (!)
                ProfileInfo[] coreProfileInfo;
                using (var stream = source.LoadArtifactByName("profiles-types.xml"))
                {
                    // var coreDefs = EnumerateBundleStream<StructureDefinition>(stream).ToList();
                    // expandCoreProfilesDerivedFrom(coreDefs, null);

                    var coreDefs = enumerateBundleStream<StructureDefinition>(stream);
                    coreProfileInfo = coreDefs.Select(sd => new ProfileInfo() { Url = sd.Url, BaseDefinition = sd.BaseDefinition }).ToArray();
                }
                expandStructuresBasedOn(resolver, coreProfileInfo, null);
            }
            finally
            {
                _generator.PrepareElement -= elementHandler;
            }
            Assert.IsTrue(result);
        }

        struct ProfileInfo { public string Url; public string BaseDefinition; }

        void expandStructuresBasedOn(IResourceResolver resolver, ProfileInfo[] profileInfo, string baseUrl)
        {
            var derivedStructures = profileInfo.Where(pi => pi.BaseDefinition == baseUrl);
            if (derivedStructures.Any())
            {
                Debug.WriteLineIf(derivedStructures.Any(), "Expand structures derived from: '{0}'".FormatWith(baseUrl));
                foreach (var info in derivedStructures)
                {
                    var sd = resolver.FindStructureDefinition(info.Url);
                    Assert.IsNotNull(sd);
                    updateSnapshot(sd);
                    expandStructuresBasedOn(resolver, profileInfo, sd.Url);
                }
            }
        }

        void updateSnapshot(StructureDefinition sd)
        {
            Assert.IsNotNull(sd);
            Debug.Print("Profile: '{0}' : '{1}'".FormatWith(sd.Url, sd.BaseDefinition));
            // Important! Must expand original instances, not clones!
            // var original = sd.DeepCopy() as StructureDefinition;
            _generator.Update(sd);
            // result &= verifyElementBase(original, entry);
            dumpOutcome(_generator.Outcome);
            dumpBaseElems(sd.Snapshot.Element);
        }

        // Verify ElementDefinition.Base components
        bool verifyElementBase(StructureDefinition original, StructureDefinition expanded)
        {
            var originalElems = original.HasSnapshot ? original.Snapshot.Element : new List<ElementDefinition>();
            var expandedElems = expanded.HasSnapshot ? expanded.Snapshot.Element : new List<ElementDefinition>();
            var isConstraint = expanded.Derivation == StructureDefinition.TypeDerivationRule.Constraint;
            Debug.Print("Original has {0} elements, expanded has {1} elements...".FormatWith(originalElems.Count, expandedElems.Count));

            // dumpBasePaths(original);

            bool verified = false;
            if (expandedElems.Count < originalElems.Count)
            {
                for (int i = 0; i < originalElems.Count; i++)
                {
                    var elem = originalElems[i];
                    var match = expandedElems.Any(e => e.Path == elem.Path);
                    if (!match)
                    {
                        Debug.Print("{0} has not been expanded...".FormatWith(elem.Path));
                    }
                }
            }
            else if (expandedElems.Count == originalElems.Count)
            {
                verified = true;

                var rootElemName = expandedElems[0].Path;

                //var baseProfileUrl = expanded.Base;
                //var baseProfile = baseProfileUrl != null ? _testResolver.FindStructureDefinition(baseProfileUrl) : null;
                //var baseRootElemName = baseProfile != null && baseProfile.Snapshot != null ? baseProfile.Snapshot.Element[0].Path : null;
                //if (expandedElems.Count > 0 && baseRootElemName != null)
                //{
                //    verified &= verifyBasePath(expandedElems[0], originalElems[0], baseRootElemName);
                //}

                if (expanded.Kind == StructureDefinition.StructureDefinitionKind.PrimitiveType)
                {
                    if (rootElemName != "Element")
                    {
                        verified &= verifyBasePath(expandedElems[0], originalElems[0], "Element");
                    }

                    if (rootElemName != "Element" && expandedElems.Count > 2)
                    {
                        verified &= verifyBasePath(expandedElems[1], originalElems[1], "Element.id");
                        verified &= verifyBasePath(expandedElems[2], originalElems[2], "Element.extension");
                    }
                }
                else if (expanded.Kind == StructureDefinition.StructureDefinitionKind.ComplexType)
                {
                    // TODO: verify that this is correct (I think so given the others in this context)
                    verified &= verifyBasePath(expandedElems[1], originalElems[1], "Element.id");
                    verified &= verifyBasePath(expandedElems[2], originalElems[2], "Element.extension");
                }
                else if (expanded.Kind == StructureDefinition.StructureDefinitionKind.Resource)
                {
                    if (rootElemName != "Resource")
                    {
                        verified &= verifyBasePath(expandedElems[0], originalElems[0], "Resource");
                    }

                    if (rootElemName != "Resource" && expandedElems.Count > 4)
                    {
                        verified &= verifyBasePath(expandedElems[1], originalElems[1], "Resource.id");
                        verified &= verifyBasePath(expandedElems[2], originalElems[2], "Resource.meta");
                        verified &= verifyBasePath(expandedElems[3], originalElems[3], "Resource.implicitRules");
                        verified &= verifyBasePath(expandedElems[4], originalElems[4], "Resource.language");
                    }
                    if (rootElemName != "DomainResource" && expandedElems.Count > 8)
                    {
                        verified &= verifyBasePath(expandedElems[5], originalElems[5], "DomainResource.text");
                        verified &= verifyBasePath(expandedElems[6], originalElems[6], "DomainResource.contained");
                        verified &= verifyBasePath(expandedElems[7], originalElems[7], "DomainResource.extension");
                        verified &= verifyBasePath(expandedElems[8], originalElems[8], "DomainResource.modifierExtension");
                    }
                    for (int i = 9; i < expandedElems.Count; i++)
                    {
                        var path = expandedElems[i].Path;
                        if (path.EndsWith(".id"))
                        {
                            verified &= verifyBasePath(expandedElems[i], originalElems[i], "Element.id");
                        }
                        else if (path.EndsWith(".extension"))
                        {
                            verified &= verifyBasePath(expandedElems[i], originalElems[i], "Element.extension");
                        }
                        else if (path.EndsWith(".modifierExtension"))
                        {
                            verified &= verifyBasePath(expandedElems[i], originalElems[i], "BackboneElement.modifierExtension");
                        }
                        else
                        {
                            if (!isConstraint)
                            {
                                // New resource element
                                verified &= verifyBasePath(expandedElems[i], originalElems[i], isConstraint ? expandedElems[i].Path : null);
                                verified &= verifyBasePath(originalElems[i], originalElems[i], isConstraint ? originalElems[i].Path : null);
                            }
                        }
                    }
                }

                if (isConstraint)
                {
                    for (int i = 0; i < expandedElems.Count; i++)
                    {
                        if (originalElems[i].Base == null) { verified = false; Debug.WriteLine("ORIGINAL: Path = {0}  => BASE IS MISSING".FormatWith(originalElems[i].Path)); }
                        if (expandedElems[i].Base == null) { verified = false; Debug.WriteLine("EXPANDED: Path = {0}  => BASE IS MISSING".FormatWith(expandedElems[i].Path)); }
                    }
                }


            }
            return verified;
        }

        static bool verifyBasePath(ElementDefinition elem, ElementDefinition orgElem, string path = "")
        {
            bool result = false;
            if (!string.IsNullOrEmpty(path))
            {
                // Assert.IsNotNull(elem.Base);
                // Assert.AreEqual(path, elem.Base.Path);

                // Assert.IsNotNull(baseElem.Base);
                // Assert.AreEqual(path, baseElem.Base.Path);

                result = elem.Base != null && path == elem.Base.Path;

                Debug.WriteLineIf(elem.Base == null, "EXPANDED: Path = {0}  => BASE IS MISSING".FormatWith(elem.Path));
                Debug.WriteLineIf(orgElem.Base == null, "ORIGINAL: Path = {0}  => BASE IS MISSING".FormatWith(orgElem.Path));

                Debug.WriteLineIf(elem.Base != null && path != elem.Base.Path, "EXPANDED: Path = {0} Base = {1} != {2} => INVALID BASE PATH".FormatWith(elem.Path, elem.Base != null ? elem.Base.Path : null, path));
                Debug.WriteLineIf(orgElem.Base != null && path != orgElem.Base.Path, "ORIGINAL: Path = {0} Base = {1} != {2} => INVALID BASE PATH".FormatWith(orgElem.Path, orgElem.Base != null ? orgElem.Base.Path : null, path));
            }
            else
            {
                // New resource element
                // Assert.IsNull(elem.Base);
                // Assert.IsNull(baseElem.Base);

                result = elem.Base == null;

                Debug.WriteLineIf(elem.Base != null, "EXPANDED: Path = {0} Base = {1} != '' => BASE SHOULD BE NULL".FormatWith(elem.Path, elem.Base != null ? elem.Base.Path : null, path));
                Debug.WriteLineIf(orgElem.Base != null, "ORIGINAL: Path = {0} Base = {1} != '' => BASE SHOULD BE NULL".FormatWith(orgElem.Path, orgElem.Base != null ? orgElem.Base.Path : null, path));

            }
            return result;
        }

        // [WMR 20161207] NEW
        // Verify reslicing order
        [TestMethod]
        public void TestReslicingOrder()
        {
            var dirSource = new DirectorySource("TestData/validation", includeSubdirectories: false);
            var sd = dirSource.FindStructureDefinition("http://example.com/StructureDefinition/patient-telecom-reslice-ek");
            Assert.IsNotNull(sd);

            //Patient.telecom : ''
            //Patient.telecom : 'phone'
            //Patient.telecom : 'email'
            //Patient.telecom : 'email/home'
            //Patient.telecom : 'email/work'
            //Patient.telecom : 'other'
            //Patient.telecom : 'other/home'
            //Patient.telecom : 'other/work'

            // Verify original differential - defines reslicing
            Debug.Print("Verify differential...");
            var diffNav = ElementDefinitionNavigator.ForDifferential(sd);
            assertPatientTelecomReslice(diffNav);

            StructureDefinition expanded;
            generateSnapshotAndCompare(sd, out expanded);

            Debug.Print("Verify snapshot...");
            var snapNav = ElementDefinitionNavigator.ForSnapshot(expanded);
            assertPatientTelecomReslice(snapNav);
        }

        void assertPatientTelecomReslice(ElementDefinitionNavigator nav)
        {
            Assert.IsTrue(nav.MoveToFirstChild());  // Patient

            if (ElementDefinitionNavigator.IsRootPath(nav.Path))
            {
                Assert.IsTrue(nav.MoveToChild("telecom"));
            }

            var bm = nav.Bookmark();
            do
            {
                Debug.Print($"{nav.Path} : '{nav.Current.SliceName}'");
            } while (nav.MoveToNext("telecom"));
            nav.ReturnToBookmark(bm);

            // Patient.telecom - slicing introduction
            Assert.IsTrue(nav.Path == "Patient.telecom");
            Assert.IsNotNull(nav.Current.Slicing);

            // Patient.telecom - slice "phone"
            Assert.IsTrue(nav.MoveToNext());
            Assert.IsTrue(nav.Path == "Patient.telecom");
            Assert.IsTrue(nav.Current.SliceName == "phone");

            // Patient.telecom - slice "email"
            Assert.IsTrue(nav.MoveToNext());
            Assert.IsTrue(nav.Path == "Patient.telecom");
            Assert.IsTrue(nav.Current.SliceName == "email");

            // Patient.telecom - reslice "email/home"
            Assert.IsTrue(nav.MoveToNext());
            Assert.IsTrue(nav.Path == "Patient.telecom");
            Assert.IsTrue(nav.Current.SliceName == "email/home");

            // Patient.telecom - reslice "email/work"
            Assert.IsTrue(nav.MoveToNext());
            Assert.IsTrue(nav.Path == "Patient.telecom");
            Assert.IsTrue(nav.Current.SliceName == "email/work");

            // Patient.telecom - slice "other"
            Assert.IsTrue(nav.MoveToNext());
            Assert.IsTrue(nav.Path == "Patient.telecom");
            Assert.IsTrue(nav.Current.SliceName == "other");

            // Patient.telecom - reslice "other/home"
            Assert.IsTrue(nav.MoveToNext());
            Assert.IsTrue(nav.Path == "Patient.telecom");
            Assert.IsTrue(nav.Current.SliceName == "other/home");

            // Patient.telecom - reslice "other/work"
            Assert.IsTrue(nav.MoveToNext());
            Assert.IsTrue(nav.Path == "Patient.telecom");
            Assert.IsTrue(nav.Current.SliceName == "other/work");
        }


        // [WMR 20161207] DEBUGGING
        // List all complex extensions that are available in the TestData folder

        // http://hl7.org/fhir/StructureDefinition/cqif-basic-codeSystem : 'TestData/snapshot-test/extensions\extension-cqif-basic-codesystem.xml'
        // http://hl7.org/fhir/StructureDefinition/cqif-basic-contributor : 'TestData/snapshot-test/extensions\extension-cqif-basic-contributor.xml'
        // http://hl7.org/fhir/StructureDefinition/cqif-basic-data : 'TestData/snapshot-test/extensions\extension-cqif-basic-data.xml'
        // http://hl7.org/fhir/StructureDefinition/cqif-basic-guidance-action : 'TestData/snapshot-test/extensions\extension-cqif-basic-guidance-action.xml'
        // http://hl7.org/fhir/StructureDefinition/cqif-basic-guidance-trigger : 'TestData/snapshot-test/extensions\extension-cqif-basic-guidance-trigger.xml'
        // http://hl7.org/fhir/StructureDefinition/cqif-basic-library : 'TestData/snapshot-test/extensions\extension-cqif-basic-library.canonical.xml'
        // http://hl7.org/fhir/StructureDefinition/cqif-basic-model : 'TestData/snapshot-test/extensions\extension-cqif-basic-model.xml'
        // http://hl7.org/fhir/StructureDefinition/cqif-basic-parameter : 'TestData/snapshot-test/extensions\extension-cqif-basic-parameter.xml'
        // http://hl7.org/fhir/StructureDefinition/cqif-basic-relatedResource : 'TestData/snapshot-test/extensions\extension-cqif-basic-relatedresource.xml'
        // http://hl7.org/fhir/StructureDefinition/cqif-basic-valueSet : 'TestData/snapshot-test/extensions\extension-cqif-basic-valueset.xml'
        // http://hl7.org/fhir/StructureDefinition/encounter-relatedCondition : 'TestData/snapshot-test/extensions\extension-encounter-relatedcondition.xml'
        // http://hl7.org/fhir/StructureDefinition/family-member-history-genetics-parent : 'TestData/snapshot-test/extensions\extension-family-member-history-genetics-parent.xml'
        // http://hl7.org/fhir/StructureDefinition/gao-extension-item : 'TestData/snapshot-test/extensions\extension-gao-extension-item.canonical.xml'
        // http://hl7.org/fhir/StructureDefinition/goal-target : 'TestData/snapshot-test/extensions\extension-goal-target.xml'
        // http://hl7.org/fhir/StructureDefinition/patient-clinicalTrial : 'TestData/snapshot-test/extensions\extension-patient-clinicaltrial.xml'
        // http://hl7.org/fhir/StructureDefinition/patient-nationality : 'TestData/snapshot-test/extensions\extension-patient-nationality.xml'
        // http://hl7.org/fhir/StructureDefinition/qicore-adverseevent-cause : 'TestData/snapshot-test/extensions\extension-qicore-adverseevent-cause.xml'
        // http://hl7.org/fhir/StructureDefinition/questionnaire-enableWhen : 'TestData/snapshot-test/extensions\extension-questionnaire-enablewhen.xml'

        [TestMethod]
        public void FindComplexTestExtensions()
        {
            Debug.WriteLine("Complex extension in TestData folder:");
            var dirSource = new DirectorySource("TestData/snapshot-test/extensions", includeSubdirectories: false);
            var uris = dirSource.ListResourceUris(ResourceType.StructureDefinition);
            foreach (var uri in uris)
            {
                var sd = dirSource.FindStructureDefinition(uri);
                if (sd.IsExtension)
                {
                    if (sd.Differential.Element.Any(e => e.Path.StartsWith("Extension.extension.", StringComparison.Ordinal)))
                    {
                        var orgInfo = sd.Annotation<OriginInformation>();
                        Debug.WriteLine($"{uri} : '{orgInfo?.Origin}'");
                    }
                }
            }
        }

        // Ewout: type slices cannot contain renamed elements!
        static StructureDefinition ObservationTypeSliceProfile => new StructureDefinition()
        {
            Type = FHIRAllTypes.Observation.GetLiteral(),
            BaseDefinition = ModelInfo.CanonicalUriForFhirCoreType(FHIRAllTypes.Observation),
            Name = "MyTestObservation",
            Url = "http://example.org/fhir/StructureDefinition/MyTestObservation",
            Derivation = StructureDefinition.TypeDerivationRule.Constraint,
            Differential = new StructureDefinition.DifferentialComponent()
            {
                Element = new List<ElementDefinition>()
                {
                    new ElementDefinition("Observation.value[x]")
                    {
                        Slicing = new ElementDefinition.SlicingComponent()
                        {
                            // Discriminator = new string[] { "@type" },
                            Discriminator = new ElementDefinition.DiscriminatorComponent[]
                                { new ElementDefinition.DiscriminatorComponent
                                    { Type = ElementDefinition.DiscriminatorType.Type }
                                }.ToList(),
                            Ordered = false,
                            Rules = ElementDefinition.SlicingRules.Open
                        }
                    }
                    ,new ElementDefinition("Observation.value[x]")
                    {
                        Type = new List<ElementDefinition.TypeRefComponent>()
                        {
                            new ElementDefinition.TypeRefComponent() { Code = FHIRAllTypes.String.GetLiteral() }
                        }
                    }
                }
            }
        };

        [Conditional("DEBUG")]
        void dumpElements(IEnumerable<ElementDefinition> elements, string header = null)
        {
            Debug.WriteLineIf(!string.IsNullOrEmpty(header), header);
            foreach (var elem in elements)
            {
                if (elem.SliceName != null)
                {
                    Debug.Print(elem.Path + " : '" + elem.SliceName + "'");
                }
                else
                {
                    Debug.Print(elem.Path);
                }
            }
        }

        [TestMethod]
        public void TestTypeSlicing()
        {
            // Create a profile with a type slice: { value[x], value[x] : String }
            var profile = ObservationTypeSliceProfile;

            var resolver = new InMemoryProfileResolver(profile);
            var multiResolver = new MultiResolver(_testResolver, resolver);
            _generator = new SnapshotGenerator(multiResolver);
            StructureDefinition expanded = null;

            generateSnapshotAndCompare(profile, out expanded);
            Assert.IsNotNull(expanded);
            Assert.IsTrue(expanded.HasSnapshot);

            dumpElements(expanded.Snapshot.Element.Where(e => e.Path.StartsWith("Observation.value")), "[1] Observation.value slice:");

            var nav = new ElementDefinitionNavigator(expanded);
            Assert.IsTrue(nav.MoveToFirstChild());
            Assert.AreEqual(nav.Path, "Observation");
            Assert.IsTrue(nav.MoveToChild("value[x]"));
            Assert.IsNotNull(nav.Current.Slicing);
            Assert.IsTrue(nav.MoveToNext());
            Assert.AreEqual(nav.PathName, "value[x]"); // valueString
            Assert.AreEqual(nav.Current.Type.FirstOrDefault().Code, FHIRAllTypes.String.GetLiteral());

            // Add an additional type slice: { value[x], value[x] : String, value[x] : CodeableConcept }
            profile.Differential.Element.Add(
                new ElementDefinition("Observation.value[x]")
                {
                    Type = new List<ElementDefinition.TypeRefComponent>()
                    {
                        new ElementDefinition.TypeRefComponent() { Code = FHIRAllTypes.CodeableConcept.GetLiteral() }
                    }
                }
            );

            generateSnapshotAndCompare(profile, out expanded);
            Assert.IsNotNull(expanded);
            Assert.IsTrue(expanded.HasSnapshot);

            dumpElements(expanded.Snapshot.Element.Where(e => e.Path.StartsWith("Observation.value")), "[2] Observation.value slice:");

            nav = new ElementDefinitionNavigator(expanded);
            Assert.IsTrue(nav.MoveToFirstChild());
            Assert.AreEqual(nav.Path, "Observation");
            Assert.IsTrue(nav.MoveToChild("value[x]"));
            Assert.IsTrue(nav.MoveToNext());
            Assert.AreEqual(nav.PathName, "value[x]"); // valueString
            Assert.AreEqual(nav.Current.Type.FirstOrDefault()?.Code, FHIRAllTypes.String.GetLiteral());
            Assert.IsTrue(nav.MoveToNext());
            Assert.AreEqual(nav.PathName, "value[x]"); // valueCodeableConcept
            Assert.AreEqual(nav.Current.Type.FirstOrDefault()?.Code, FHIRAllTypes.CodeableConcept.GetLiteral());
        }

        [TestMethod]
        public void TestMissingDifferential()
        {
            // Create a profile without a differential
            var profile = ObservationTypeSliceProfile;
            profile.Differential = null;

            var resolver = new InMemoryProfileResolver(profile);
            var multiResolver = new MultiResolver(_testResolver, resolver);
            _generator = new SnapshotGenerator(multiResolver);
            StructureDefinition expanded = null;

            generateSnapshotAndCompare(profile, out expanded);
            Assert.IsNotNull(expanded);
            Assert.IsTrue(expanded.HasSnapshot);

            dumpElements(expanded.Snapshot.Element);
        }

        [TestMethod]
        public void TestUnresolvedBaseProfile()
        {
            // Create a profile with an unresolved base profile reference
            var profile = ObservationTypeSliceProfile;
            profile.BaseDefinition = "http://example.org/fhir/StructureDefinition/missing";

            var resolver = new InMemoryProfileResolver(profile);
            var multiResolver = new MultiResolver(_testResolver, resolver);
            _generator = new SnapshotGenerator(multiResolver);
            StructureDefinition expanded = null;

            generateSnapshotAndCompare(profile, out expanded);
            Assert.IsNotNull(expanded);
            Assert.IsFalse(expanded.HasSnapshot);
            var outcome = _generator.Outcome;
            Assert.IsNotNull(outcome);
            Assert.IsNotNull(outcome.Issue);
            Assert.AreEqual(outcome.Issue.Count, 1);
            assertIssue(outcome.Issue[0], Issue.UNAVAILABLE_REFERENCED_PROFILE, profile.BaseDefinition);
        }

        static StructureDefinition ObservationTypeResliceProfile => new StructureDefinition()
        {
            Type = FHIRAllTypes.Observation.GetLiteral(),
            BaseDefinition = ObservationTypeSliceProfile.Url,
            Name = "MyDerivedTestObservation",
            Url = "http://example.org/fhir/StructureDefinition/MyDerivedTestObservation",
            Derivation = StructureDefinition.TypeDerivationRule.Constraint,
            Differential = new StructureDefinition.DifferentialComponent()
            {
                Element = new List<ElementDefinition>()
                {
                    new ElementDefinition("Observation.value[x]")
                    {
                        Slicing = new ElementDefinition.SlicingComponent()
                        {
                            // Discriminator = new string[] { "@type" },
                            Discriminator = new ElementDefinition.DiscriminatorComponent[]
                                { new ElementDefinition.DiscriminatorComponent
                                    { Type = ElementDefinition.DiscriminatorType.Type }
                                }.ToList(),
                            Ordered = false,
                            Rules = ElementDefinition.SlicingRules.Open
                        }
                    }
                    // Constraint on existing type slice value[x] : String
                    ,new ElementDefinition("Observation.value[x]")
                    {
                        Max = "1", // New constraint
                        Type = new List<ElementDefinition.TypeRefComponent>()
                        {
                            new ElementDefinition.TypeRefComponent() { Code = FHIRAllTypes.String.GetLiteral() }
                        }
                    }
                    
                    // Remove existing type slice value[x]: CodeableConcept

                    // Add a new type slice value[x]: Integer
                    ,new ElementDefinition("Observation.value[x]")
                    {
                        Type = new List<ElementDefinition.TypeRefComponent>()
                        {
                            new ElementDefinition.TypeRefComponent() { Code = FHIRAllTypes.Integer.GetLiteral() }
                        }
                    },
                }
            }
        };

        [TestMethod]
        public void TestTypeReslicing()
        {
            // Create a derived profile from a base profile with a type slice
            var profile = ObservationTypeResliceProfile;
            var baseProfile = ObservationTypeSliceProfile;

            var resources = new IConformanceResource[] { profile, baseProfile };
            var resolver = new InMemoryProfileResolver(resources);
            var multiResolver = new MultiResolver(_testResolver, resolver);
            _generator = new SnapshotGenerator(multiResolver);
            StructureDefinition expanded = null;

            generateSnapshotAndCompare(profile, out expanded);
            Assert.IsNotNull(expanded);
            Assert.IsTrue(expanded.HasSnapshot);

            dumpElements(expanded.Snapshot.Element.Where(e => e.Path.StartsWith("Observation.value")), "[1] Observation.value reslice:");

            var nav = new ElementDefinitionNavigator(expanded);
            Assert.IsTrue(nav.MoveToFirstChild());
            Assert.AreEqual(nav.Path, "Observation");
            Assert.IsTrue(nav.MoveToChild("value[x]"));
            Assert.IsTrue(nav.MoveToNext());
            Assert.AreEqual(nav.PathName, "value[x]"); // valueString
            Assert.AreEqual(nav.Current.Type.FirstOrDefault()?.Code, FHIRAllTypes.String.GetLiteral());
            // Derived profile REMOVES existing CodeableConcept type slice and introduces a new Integer type slice
            // Note: special rules for element types allow removal of inherited collection items
            Assert.IsTrue(nav.MoveToNext());
            Assert.AreEqual(nav.PathName, "value[x]"); // valueCodeableConcept
            Assert.AreEqual(nav.Current.Type.FirstOrDefault()?.Code, FHIRAllTypes.Integer.GetLiteral());
        }

        // Choice type constraint, with element renaming
        static StructureDefinition ObservationTypeConstraintProfile => new StructureDefinition()
        {
            Type = FHIRAllTypes.Observation.GetLiteral(),
            BaseDefinition = ModelInfo.CanonicalUriForFhirCoreType(FHIRAllTypes.Observation),
            Name = "MyTestObservation",
            Url = "http://example.org/fhir/StructureDefinition/MyTestObservation",
            Derivation = StructureDefinition.TypeDerivationRule.Constraint,
            Differential = new StructureDefinition.DifferentialComponent()
            {
                Element = new List<ElementDefinition>()
                {
                    // No slicing introduction
                    // Only single element is allowed (this is NOT a slice!)
                    // Element is renamed
                    new ElementDefinition("Observation.valueString")
                    {
                        Type = new List<ElementDefinition.TypeRefComponent>()
                        {
                            new ElementDefinition.TypeRefComponent() { Code = FHIRAllTypes.String.GetLiteral() }
                        }
                    }
                }
            }
        };

        [TestMethod]
        public void TestChoiceTypeConstraint()
        {
            // Create a profile with a choice type constraint: value[x] => valueString
            var profile = ObservationTypeConstraintProfile;

            var resolver = new InMemoryProfileResolver(profile);
            var multiResolver = new MultiResolver(_testResolver, resolver);
            _generator = new SnapshotGenerator(multiResolver);
            StructureDefinition expanded = null;

            generateSnapshotAndCompare(profile, out expanded);
            Assert.IsNotNull(expanded);
            Assert.IsTrue(expanded.HasSnapshot);

            dumpElements(expanded.Snapshot.Element.Where(e => e.Path.StartsWith("Observation.value")), "Observation.value choice type constraint:");

            var nav = new ElementDefinitionNavigator(expanded);
            Assert.IsTrue(nav.MoveToFirstChild());
            Assert.AreEqual(nav.Path, "Observation");
            Assert.IsFalse(nav.MoveToChild("value[x]")); // Should also be renamed to valueString in snapshot
            Assert.IsTrue(nav.MoveToChild("valueString"));
            Assert.IsNull(nav.Current.Slicing);
            Assert.AreEqual(nav.Current.Type.FirstOrDefault().Code, FHIRAllTypes.String.GetLiteral());
        }

        [TestMethod]
        public void TestInvalidChoiceTypeConstraints()
        {
            // Create a profile with multiple (invalid!) choice type constraint: value[x] => { valueString, valueInteger }
            var profile = ObservationTypeConstraintProfile;
            profile.Differential.Element.Add(
                    new ElementDefinition("Observation.valueInteger")
                    {
                        Type = new List<ElementDefinition.TypeRefComponent>()
                        {
                            new ElementDefinition.TypeRefComponent() { Code = FHIRAllTypes.Integer.GetLiteral() }
                        }
                    }
            );

            var resolver = new InMemoryProfileResolver(profile);
            var multiResolver = new MultiResolver(_testResolver, resolver);
            _generator = new SnapshotGenerator(multiResolver);
            StructureDefinition expanded = null;

            generateSnapshotAndCompare(profile, out expanded);
            Assert.IsNotNull(expanded);
            Assert.IsTrue(expanded.HasSnapshot);

            dumpElements(expanded.Snapshot.Element.Where(e => e.Path.StartsWith("Observation.value")), "Observation.value choice type constraint:");
            var outcome = _generator.Outcome;
            dumpOutcome(outcome);

            var nav = new ElementDefinitionNavigator(expanded);
            Assert.IsTrue(nav.MoveToFirstChild());
            Assert.AreEqual(nav.Path, "Observation");
            Assert.IsFalse(nav.MoveToChild("value[x]")); // Should also be renamed to valueString in snapshot
            Assert.IsTrue(nav.MoveToChild("valueString"));
            Assert.IsNull(nav.Current.Slicing);
            Assert.AreEqual(nav.Current.Type.FirstOrDefault().Code, FHIRAllTypes.String.GetLiteral());

            Assert.IsTrue(nav.MoveToNext("valueInteger"));
            Assert.IsNull(nav.Current.Slicing);
            Assert.AreEqual(nav.Current.Type.FirstOrDefault().Code, FHIRAllTypes.Integer.GetLiteral());

            Assert.IsNotNull(outcome);
            Assert.AreEqual(1, outcome.Issue.Count);
            assertIssue(outcome.Issue[0], SnapshotGenerator.PROFILE_ELEMENTDEF_INVALID_CHOICE_CONSTRAINT);
        }

        static StructureDefinition ClosedExtensionSliceObservationProfile => new StructureDefinition()
        {
            Type = FHIRAllTypes.Observation.GetLiteral(),
            BaseDefinition = ModelInfo.CanonicalUriForFhirCoreType(FHIRAllTypes.Observation),
            Name = "MyTestObservation",
            Url = "http://example.org/fhir/StructureDefinition/MyTestObservation",
            Derivation = StructureDefinition.TypeDerivationRule.Constraint,
            Differential = new StructureDefinition.DifferentialComponent()
            {
                Element = new List<ElementDefinition>()
                {
                    new ElementDefinition("Observation.extension")
                    {
                        Slicing = new ElementDefinition.SlicingComponent()
                        {
                            Rules = ElementDefinition.SlicingRules.Closed
                        }
                    }
                }
            }
        };

        [TestMethod]
        public void TestEmptyClosedExtensionSlice()
        {
            var profile = ClosedExtensionSliceObservationProfile;

            var resolver = new InMemoryProfileResolver(profile);
            var multiResolver = new MultiResolver(_testResolver, resolver);
            _generator = new SnapshotGenerator(multiResolver);
            StructureDefinition expanded = null;

            generateSnapshotAndCompare(profile, out expanded);
            Assert.IsNotNull(expanded);
            Assert.IsTrue(expanded.HasSnapshot);

            // dumpElements(expanded.Snapshot.Element.Where(e => e.Path.StartsWith("Observation.extension")), "Observation.extension constraint:");
            var outcome = _generator.Outcome;
            dumpOutcome(outcome);

            var elem = expanded.Snapshot.Element.Find(e => e.Path == "Observation.extension");
            Assert.IsNotNull(elem);
            Assert.IsNotNull(elem.Slicing);
            Assert.AreEqual(ElementDefinition.SlicingRules.Closed, elem.Slicing.Rules);
        }

        [TestMethod]
        public void TestSlicingEntryWithChilren()
        {
            var sd = _testResolver.FindStructureDefinition(@"http://example.org/StructureDefinition/DocumentComposition");
            Assert.IsNotNull(sd);

            // dumpReferences(sd);

            StructureDefinition expanded;
            generateSnapshotAndCompare(sd, out expanded);

            dumpOutcome(_generator.Outcome);
            dumpElements(expanded.Snapshot.Element);

            // Verify that the snapshot includes the merged children of the slice entry element
            var verifier = new ElementVerifier(expanded, _settings);
            verifier.VerifyElement("Composition.section", null);
            verifier.AssertSlicing(new string[] { "code" }, ElementDefinition.SlicingRules.Open, false);
            verifier.VerifyElement("Composition.section.title", null);
            verifier.VerifyElement("Composition.section.code", null);
            Assert.IsNotNull(verifier.CurrentElement.Binding);
            Assert.AreEqual(BindingStrength.Required, verifier.CurrentElement.Binding.Strength);
            Assert.AreEqual("http://example.org/ValueSet/SectionTitles", (verifier.CurrentElement.Binding.ValueSet as ResourceReference)?.Reference);
        }

        [TestMethod]
        public void TestObservationProfileWithExtensions() => testObservationProfileWithExtensions(false);

        [TestMethod]
        public void TestObservationProfileWithExtensions_ExpandAll() => testObservationProfileWithExtensions(true);

        void testObservationProfileWithExtensions(bool expandAll)
        {
            // Same as TestObservationProfileWithExtensions, but with full expansion of all complex elements (inc. extensions!)

            // var obs = _testResolver.FindStructureDefinition(@"http://example.org/fhir/StructureDefinition/MyCustomObservation");
            var obs = _testResolver.FindStructureDefinition(@"http://example.org/fhir/StructureDefinition/MyCustomObservation3");
            Assert.IsNotNull(obs);

            StructureDefinition expanded;
            _generator = new SnapshotGenerator(_testResolver, _settings);
            _generator.PrepareElement += elementHandler;
            if (expandAll)
            {
                _generator.BeforeExpandElement += beforeExpandElementHandler;
            }
            try
            {
                generateSnapshotAndCompare(obs, out expanded);
            }
            finally
            {
                _generator.PrepareElement -= elementHandler;
                if (expandAll)
                {
                    _generator.BeforeExpandElement -= beforeExpandElementHandler;
                }
            }

            dumpOutcome(_generator.Outcome);

            var elems = expanded.Snapshot.Element;
            dumpElements(elems);
            dumpBaseElems(elems);

            // Verify that the snapshot contains three extension elements 
            var obsExtensions = elems.Where(e => e.Path == "Observation.extension").ToList();
            Assert.IsNotNull(obsExtensions);
            Assert.AreEqual(4, obsExtensions.Count); // 1 extension slice + 3 extensions

            var extSliceElem = obsExtensions[0];
            Assert.IsNotNull(extSliceElem);
            Assert.IsNotNull(extSliceElem.Slicing);
            Assert.AreEqual("url", extSliceElem.Slicing.Discriminator.FirstOrDefault().Path);

            var labelExtElem = obsExtensions[1];
            Assert.IsNotNull(labelExtElem);
            Assert.AreEqual(@"http://example.org/fhir/StructureDefinition/ObservationLabelExtension", labelExtElem.Type.FirstOrDefault().Profile);

            var locationExtElem = obsExtensions[2];
            Assert.IsNotNull(locationExtElem);
            Assert.AreEqual(@"http://example.org/fhir/StructureDefinition/ObservationLocationExtension", locationExtElem.Type.FirstOrDefault().Profile);

            var otherExtElem = obsExtensions[3];
            Assert.IsNotNull(otherExtElem);
            Assert.AreEqual(@"http://example.org/fhir/StructureDefinition/SomeOtherExtension", otherExtElem.Type.FirstOrDefault().Profile);

            var labelExt = _testResolver.FindStructureDefinition(@"http://example.org/fhir/StructureDefinition/ObservationLabelExtension");
            Assert.IsNotNull(labelExt);
            if (expandAll) { Assert.AreEqual(true, labelExt.HasSnapshot); }

            var locationExt = _testResolver.FindStructureDefinition(@"http://example.org/fhir/StructureDefinition/ObservationLocationExtension");
            Assert.IsNotNull(locationExt);
            if (expandAll) { Assert.AreEqual(true, locationExt.HasSnapshot); }

            // Third extension element maps to an unresolved extension definition
            var otherExt = _testResolver.FindStructureDefinition(@"http://example.org/fhir/StructureDefinition/SomeOtherExtension");
            Assert.IsNull(otherExt);

            // Now verify the snapshot
            // First two extension elements should have been merged from the snapshot root Extension element of the associated extension definition 
            var coreExtension = _testResolver.FindStructureDefinitionForCoreType(FHIRAllTypes.Extension);
            Assert.IsNotNull(coreExtension);
            Assert.IsTrue(coreExtension.HasSnapshot);
            var coreExtensionRootElem = coreExtension.Snapshot.Element[0];

            var labelExtRootElem = labelExt.Differential.Element[0];
            Assert.AreEqual(1, labelExtElem.Min);                                           // Explicit Observation profile constraint
            Assert.AreEqual(labelExtRootElem.Max, labelExtElem.Max);                        // Inherited from external ObservationLabelExtension root element
            Assert.AreEqual(coreExtensionRootElem.Definition, labelExtElem.Definition);     // Inherited from Observation.extension base element
            Assert.AreEqual(labelExtRootElem.Comment, labelExtElem.Comment);              // Inherited from external ObservationLabelExtension root element
            verifyProfileExtensionBaseElement(labelExtElem);

            var locationExtRootElem = locationExt.Differential.Element[0];
            Assert.AreEqual(0, locationExtElem.Min);                                        // Inherited from external ObservationLabelExtension root element
            Assert.AreEqual("1", locationExtElem.Max);                                      // Explicit Observation profile constraint
            Assert.AreEqual(coreExtensionRootElem.Definition, locationExtElem.Definition);  // Inherited from Observation.extension base element
            Assert.AreEqual(locationExtRootElem.Comment, locationExtElem.Comment);        // Inherited from external ObservationLocationExtension root element
            verifyProfileExtensionBaseElement(locationExtElem);

            // Last (unresolved) extension element should have been merged with Observation.extension
            var coreObservation = _testResolver.FindStructureDefinitionForCoreType(FHIRAllTypes.Observation);
            Assert.IsNotNull(coreObservation);
            Assert.IsTrue(coreObservation.HasSnapshot);
            var coreObsExtensionElem = coreObservation.Snapshot.Element.FirstOrDefault(e => e.Path == "Observation.extension");
            Assert.IsNotNull(coreObsExtensionElem);
            Assert.AreEqual(1, otherExtElem.Min);                                           // Explicit Observation profile constraint
            Assert.AreEqual(coreObsExtensionElem.Max, otherExtElem.Max);                    // Inherited from Observation.extension base element
            Assert.AreEqual(coreObsExtensionElem.Definition, otherExtElem.Definition);      // Inherited from Observation.extension base element
            Assert.AreEqual(coreObsExtensionElem.Comment, otherExtElem.Comment);          // Inherited from Observation.extension base element
            verifyProfileExtensionBaseElement(coreObsExtensionElem);
        }

        void verifyProfileExtensionBaseElement(ElementDefinition extElem)
        {
            var baseElem = extElem.Annotation<BaseDefAnnotation>().BaseElementDefinition;
            Assert.IsNotNull(baseElem);
            Assert.AreEqual(baseElem.Short, extElem.Short);
            Assert.AreEqual(baseElem.Definition, extElem.Definition);
            Assert.AreEqual(baseElem.Comment, extElem.Comment);
            Assert.IsTrue(baseElem.Alias.SequenceEqual(extElem.Alias));
        }

        // [WMR 20170213] New - issue reported by Marten - cannot slice Organization.type ?
        // Specifically, snapshot generator drops the slicing component from the slice entry element
        // Explanation: Organization.type is not a list (max = 1) and not a choice type => slicing is not allowed!
        [TestMethod]
        public void TestOrganizationTypeSlice()
        {
            var org = _testResolver.FindStructureDefinition(@"http://example.org/fhir/StructureDefinition/MySlicedOrganization");
            Assert.IsNotNull(org);

            StructureDefinition expanded;
            _generator = new SnapshotGenerator(_testResolver, _settings);
            _generator.PrepareElement += elementHandler;
            try
            {
                generateSnapshotAndCompare(org, out expanded);
            }
            finally
            {
                _generator.PrepareElement -= elementHandler;
            }

            dumpOutcome(_generator.Outcome);

            var elems = expanded.Snapshot.Element;
            dumpElements(elems);
            //dumpBaseElems(elems);

            // TODO: Verify slice

        }

        // [WMR 2017024] NEW: Test for bug with snapshot expansion of ElementDefinition.Binding (reported by NHS)
        // If the diff constrains only Binding.Strength, then snapshot also contains only Binding.Strength - WRONG!
        // Expected: snapshot contains inherited properties from base, i.e. description, valueSetUri/valueSetReference
        [TestMethod]
        public void TestElementBinding()
        {
            var sd = new StructureDefinition()
            {
                Type = FHIRAllTypes.Encounter.GetLiteral(),
                BaseDefinition = ModelInfo.CanonicalUriForFhirCoreType(FHIRAllTypes.Encounter),
                Name = "MyTestEncounter",
                Url = "http://example.org/fhir/StructureDefinition/MyTestEncounter",
                Derivation = StructureDefinition.TypeDerivationRule.Constraint,
                Differential = new StructureDefinition.DifferentialComponent()
                {
                    Element = new List<ElementDefinition>()
                    {
                        new ElementDefinition("Encounter.type")
                        {

                            // Default binding on Encounter.type:
                            //
                            // <binding>
                            //   <strength value="example" />
                            //   <description value="The type of encounter" />
                            //   <valueSetReference>
                            //     <reference value="http://hl7.org/fhir/ValueSet/encounter-type" />
                            //   </valueSetReference>
                            // </binding>

                            Binding = new ElementDefinition.ElementDefinitionBindingComponent()
                            {
                                // Constrain strength from Example to Preferred
                                Strength = BindingStrength.Preferred
                            }
                        }
                    }

                }
            };

            var resolver = new InMemoryProfileResolver(sd);
            var multiResolver = new MultiResolver(_testResolver, resolver);
            _generator = new SnapshotGenerator(multiResolver);
            StructureDefinition expanded = null;

            generateSnapshotAndCompare(sd, out expanded);
            Assert.IsNotNull(expanded);
            Assert.IsTrue(expanded.HasSnapshot);

            var profileElem = expanded.Snapshot.Element.FirstOrDefault(e => e.Path == "Encounter.type");
            Assert.IsNotNull(profileElem);
            var profileBinding = profileElem.Binding;
            Assert.IsNotNull(profileBinding);

            Assert.AreEqual(BindingStrength.Preferred, profileBinding.Strength);

            var sdEncounter = _testResolver.FindStructureDefinitionForCoreType(FHIRAllTypes.Encounter);
            Assert.IsNotNull(sdEncounter);
            Assert.IsTrue(sdEncounter.HasSnapshot);

            var baseElem = sdEncounter.Snapshot.Element.FirstOrDefault(e => e.Path == "Encounter.type");
            Assert.IsNotNull(baseElem);
            var baseBinding = baseElem.Binding;
            Assert.IsNotNull(baseBinding);

            Assert.AreEqual(BindingStrength.Example, baseBinding.Strength);

            Assert.AreEqual(baseBinding.Description, profileBinding.Description);
            Assert.IsTrue(baseBinding.ValueSet.IsExactly(profileBinding.ValueSet));
        }

        // [WMR 2017024] NEW: Snapshot generator should reject profile extensions mapped to a StructureDefinition that is not an Extension definition.
        // Reported by Thomas Tveit Rosenlund: https://simplifier.net/Velferdsteknologi2/FlagVFT (geoPositions)
        // Don't expand; emit outcome issue
        [TestMethod]
        public void TestInvalidProfileExtensionTarget()
        {
            var sdLocation = new StructureDefinition()
            {
                Type = FHIRAllTypes.Location.GetLiteral(),
                BaseDefinition = ModelInfo.CanonicalUriForFhirCoreType(FHIRAllTypes.Location),
                Name = "MyTestLocation",
                Url = "http://example.org/fhir/StructureDefinition/MyTestLocation",
                Differential = new StructureDefinition.DifferentialComponent()
                {
                    Element = new List<ElementDefinition>()
                    {
                        new ElementDefinition()
                        {
                            Path = "Location.partOf",
                            Max = "0"
                        }
                    }
                }
            };

            var sdFlag = new StructureDefinition()
            {
                Type = FHIRAllTypes.Flag.GetLiteral(),
                BaseDefinition = ModelInfo.CanonicalUriForFhirCoreType(FHIRAllTypes.Flag),
                Name = "MyTestFlag",
                Url = "http://example.org/fhir/StructureDefinition/MyTestFlag",
                Differential = new StructureDefinition.DifferentialComponent()
                {
                    Element = new List<ElementDefinition>()
                    {
                        new ElementDefinition("Flag.extension")
                        {
                            Slicing = new ElementDefinition.SlicingComponent()
                            {
                                // Discriminator = new string[] { "url" },
                                Discriminator = new ElementDefinition.DiscriminatorComponent[]
                                { new ElementDefinition.DiscriminatorComponent
                                    { Type = ElementDefinition.DiscriminatorType.Value, Path = "url" }
                                }.ToList(),
                                Rules = ElementDefinition.SlicingRules.Open
                            }
                        },
                        new ElementDefinition("Flag.extension")
                        {
                            SliceName = "geopositions",
                            Type = new List<ElementDefinition.TypeRefComponent>()
                            {
                                new ElementDefinition.TypeRefComponent()
                                {
                                    Code = FHIRAllTypes.Extension.GetLiteral(),
                                    // INVALID - Map extension element to non-extension definition
                                    Profile = sdLocation.Url
                                }

                            }
                        }
                    }

                }
            };

            var resolver = new InMemoryProfileResolver(sdLocation, sdFlag);
            var multiResolver = new MultiResolver(_testResolver, resolver);
            _generator = new SnapshotGenerator(multiResolver, _settings);
            _generator.BeforeExpandElement += beforeExpandElementHandler;
            StructureDefinition expanded = null;
            try
            {
                generateSnapshotAndCompare(sdFlag, out expanded);
            }
            finally
            {
                _generator.BeforeExpandElement -= beforeExpandElementHandler;
            }

            Assert.IsNotNull(expanded);
            Assert.IsTrue(expanded.HasSnapshot);

            dumpOutcome(_generator.Outcome);

            Assert.IsNotNull(_generator.Outcome);
            Assert.IsNotNull(_generator.Outcome.Issue);
            Assert.AreEqual(1, _generator.Outcome.Issue.Count);
            assertIssue(_generator.Outcome.Issue[0], SnapshotGenerator.PROFILE_ELEMENTDEF_INVALID_PROFILE_TYPE);

            dumpElements(expanded.Snapshot.Element);
        }

        // Verify extension constraint on choice type element w/o type slice
        [TestMethod]
        public void TestZibProcedure()
        {
            var sd = _testResolver.FindStructureDefinition("http://nictiz.nl/fhir/StructureDefinition/zib-Procedure");
            Assert.IsNotNull(sd);
            assertContainsElement(sd.Differential, "Procedure.request.extension", "RequestedBy");

            StructureDefinition expanded = null;
            generateSnapshotAndCompare(sd, out expanded);
            dumpOutcome(_generator.Outcome);

            Assert.IsTrue(expanded.HasSnapshot);
            dumpElements(expanded.Snapshot.Element);

            // Verify that the snapshot contains the extension on Procedure.request (w/o type slice)
            assertContainsElement(expanded.Snapshot, "Procedure.request.extension", "RequestedBy");
        }

        // [WMR 20170306] Verify that the snapshot generator determines and merges the correct base element for slices
        // * Slice entry is based on associated element in base profile with same path (and name)
        //   Slice entry inherits constraints from base element; can only further constrain
        //   Note: Base element may be a slice entry itself, or a named slice (in case of reslicing)
        // * Named slices are based on associated element in base profile with same path and parent slice name (same name as preceding slice entry)
        //   Same base element as preceding slice entry, but without the slicing component and with min = 0 (per definition for named slices, as they can be optional)

        //
        // Example:
        //
        // Patient (base profile)
        // - Patient.identifier
        //
        // MyPatient : Patient (user profile)
        // - Patient.identifier (slice entry)     => Patient.identifier (in Base)
        // - Patient.identifier:A                 => Patient.identifier (in Base)
        // - Patient.identifier:A/1               => Patient.identifier (in Base)
        // - Patient.identifier:A/2               => Patient.identifier (in Base)
        // - Patient.identifier:B                 => Patient.identifier (in Base)
        //
        // DerivedPatient : MyPatient (derived user profile)
        // - Patient.identifier (slice entry)     => Patient.identifier (slice entry) in MyPatient
        // - Patient.identifier:A                 => Patient.identifier:A in MyPatient
        // - Patient.identifier:A/1               => Patient.identifier:A/1 in MyPatient
        // - Patient.identifier:A/2               => Patient.identifier:A/2 in MyPatient
        // - Patient.identifier:A/3               => Patient.identifier:A in MyPatient
        // - Patient.identifier:B (reslice entry) => Patient.identifier:B in MyPatient
        // - Patient.identifier:B/1               => Patient.identifier:B in MyPatient
        // - Patient.identifier:B/2               => Patient.identifier:B in MyPatient
        // - Patient.identifier:C                 => Patient.identifier in MyPatient

        static StructureDefinition SlicedPatientProfile => new StructureDefinition()
        {
            Type = FHIRAllTypes.Patient.GetLiteral(),
            BaseDefinition = ModelInfo.CanonicalUriForFhirCoreType(FHIRAllTypes.Patient),
            Name = "MySlicedPatient",
            Url = "http://example.org/fhir/StructureDefinition/MySlicedPatient",
            Derivation = StructureDefinition.TypeDerivationRule.Constraint,
            Differential = new StructureDefinition.DifferentialComponent()
            {
                Element = new List<ElementDefinition>()
                {
                    new ElementDefinition("Patient.identifier")
                    {
                        Slicing = new ElementDefinition.SlicingComponent()
                        {
                            // Discriminator = new string[] { "system" },
                            Discriminator = new ElementDefinition.DiscriminatorComponent[]
                                { new ElementDefinition.DiscriminatorComponent
                                    { Type = ElementDefinition.DiscriminatorType.Value, Path = "system" }
                                }.ToList(),
                            Ordered = false,
                            Rules = ElementDefinition.SlicingRules.Open
                        },
                        Min = 1
                    }
                    ,new ElementDefinition("Patient.identifier")
                    {
                        SliceName = "bsn",
                        Min = 1,
                        Max = "1"
                    }
                    ,new ElementDefinition("Patient.identifier")
                    {
                        SliceName = "ehr_id",
                        Max = "2"
                    }
                }
            }
        };

        [TestMethod]
        public void TestSliceBase_SlicedPatient()
        {
            var profile = SlicedPatientProfile;

            var resolver = new InMemoryProfileResolver(profile);
            var multiResolver = new MultiResolver(_testResolver, resolver);
            _generator = new SnapshotGenerator(multiResolver);
            StructureDefinition expanded = null;

            _generator.PrepareElement += elementHandler;
            try
            {
                generateSnapshotAndCompare(profile, out expanded);
            }
            finally
            {
                _generator.PrepareElement -= elementHandler;
            }
            dumpOutcome(_generator.Outcome);

            Assert.IsNotNull(expanded);
            Assert.IsTrue(expanded.HasSnapshot);

            var identifierConstraints = expanded.Snapshot.Element.Where(e => e.Path.StartsWith("Patient.identifier"));

            dumpElements(identifierConstraints, "Constraints on Patient.identifier:");

            var corePatientProfile = _testResolver.FindStructureDefinition(profile.BaseDefinition);
            Assert.IsNotNull(corePatientProfile);
            Assert.IsTrue(corePatientProfile.HasSnapshot);
            var corePatientIdentifierElem = corePatientProfile.Snapshot.Element.FirstOrDefault(e => e.Path == "Patient.identifier");
            Assert.IsNotNull(corePatientIdentifierElem);
            Debug.Print($"Base: #{corePatientIdentifierElem.GetHashCode()} '{corePatientIdentifierElem.Path}'");

            dumpBaseElems(identifierConstraints);

            var nav = ElementDefinitionNavigator.ForSnapshot(expanded);
            Assert.IsTrue(nav.MoveToFirstChild());

            // Verify slice entry
            Assert.IsTrue(nav.MoveToChild("identifier"));
            Assert.AreEqual(corePatientIdentifierElem, GetBaseElementAnnotation(nav.Current));
            Assert.IsNotNull(nav.Current.Slicing);
            Assert.IsNull(nav.Current.SliceName);
            Assert.AreEqual(1, nav.Current.Min);
            Assert.AreEqual("*", nav.Current.Max);

            // Verify slice "bsn"
            Assert.IsTrue(nav.MoveToNextSlice());
            Assert.AreEqual(corePatientIdentifierElem, GetBaseElementAnnotation(nav.Current));
            Assert.IsNull(nav.Current.Slicing);
            Assert.AreEqual("bsn", nav.Current.SliceName);
            Assert.AreEqual(1, nav.Current.Min);
            Assert.AreEqual("1", nav.Current.Max);

            // Verify slice "ehr_id"
            Assert.IsTrue(nav.MoveToNextSlice());
            Assert.AreEqual(corePatientIdentifierElem, GetBaseElementAnnotation(nav.Current));
            Assert.IsNull(nav.Current.Slicing);
            Assert.AreEqual("ehr_id", nav.Current.SliceName);
            Assert.AreEqual(0, nav.Current.Min);
            Assert.AreEqual("2", nav.Current.Max);
        }

        static StructureDefinition NationalPatientProfile => new StructureDefinition()
        {
            Type = FHIRAllTypes.Patient.GetLiteral(),
            BaseDefinition = ModelInfo.CanonicalUriForFhirCoreType(FHIRAllTypes.Patient),
            Name = "MyNationalPatient",
            Url = "http://example.org/fhir/StructureDefinition/MyNationalPatient",
            Derivation = StructureDefinition.TypeDerivationRule.Constraint,
            Differential = new StructureDefinition.DifferentialComponent()
            {
                Element = new List<ElementDefinition>()
                {
                    new ElementDefinition("Patient.identifier")
                    {
                        Comment = "NationalPatientProfile"
                    },
                    new ElementDefinition("Patient.identifier.system")
                    {
                        Min = 1
                    }
                }
            }
        };

        static StructureDefinition SlicedNationalPatientProfile => new StructureDefinition()
        {
            Type = FHIRAllTypes.Patient.GetLiteral(),
            BaseDefinition = "http://example.org/fhir/StructureDefinition/MyNationalPatient",
            Name = "SlicedNationalPatientProfile",
            Url = "http://example.org/fhir/StructureDefinition/SlicedNationalPatientProfile",
            Derivation = StructureDefinition.TypeDerivationRule.Constraint,
            Differential = new StructureDefinition.DifferentialComponent()
            {
                Element = new List<ElementDefinition>()
                {
                    new ElementDefinition("Patient.identifier")
                    {
                        Slicing = new ElementDefinition.SlicingComponent()
                        {
                            // Discriminator = new string[] { "system" },
                            Discriminator = new ElementDefinition.DiscriminatorComponent[]
                                { new ElementDefinition.DiscriminatorComponent
                                    { Type = ElementDefinition.DiscriminatorType.Value, Path = "system" }
                                }.ToList(),
                            Ordered = false,
                            Rules = ElementDefinition.SlicingRules.Open
                        },
                        Min = 1,
                        // Append to comment inherited from base
                        Comment = "...SlicedNationalPatientProfile"
                    }
                    // Slice: bsn
                    ,new ElementDefinition("Patient.identifier")
                    {
                        SliceName = "bsn",
                        Min = 1,
                        Max = "1"
                    },
                    new ElementDefinition("Patient.identifier.system")
                    {
                        Fixed = new FhirUri("http://example.org/fhir/ValueSet/bsn")
                    },
                    // Slice: ehr_id
                    new ElementDefinition("Patient.identifier")
                    {
                        SliceName = "ehr_id",
                        Max = "2",
#if false
                        // Re-slice the ehr-id
                        Slicing = new ElementDefinition.SlicingComponent()
                        {
                            Discriminator = new string[] { "use" },
                            Ordered = true,
                            Rules = ElementDefinition.SlicingRules.Closed
                        }
#endif
                    },
#if false
                    // Reslice: ehr-id/temp
                    new ElementDefinition("Patient.identifier")
                    {
                        Name = "ehr_id/temp",
                        Max = "1",
                    },
                    new ElementDefinition("Patient.identifier.use")
                    {
                        // Fixed = new Code<Identifier.IdentifierUse>(Identifier.IdentifierUse.Temp)
                        Fixed = new Code("temp")
                    }
#endif
                }
            }
        };

        [TestMethod]
        public void TestSliceBase_SlicedNationalPatient()
        {
            var baseProfile = NationalPatientProfile;
            var profile = SlicedNationalPatientProfile;

            var resolver = new InMemoryProfileResolver(baseProfile, profile);
            var multiResolver = new MultiResolver(_testResolver, resolver);
            _generator = new SnapshotGenerator(multiResolver);
            StructureDefinition expanded = null;

            _generator.PrepareElement += elementHandler;
            try
            {
                generateSnapshotAndCompare(profile, out expanded);
            }
            finally
            {
                _generator.PrepareElement -= elementHandler;
            }
            dumpOutcome(_generator.Outcome);

            Assert.IsNotNull(expanded);
            Assert.IsTrue(expanded.HasSnapshot);

            var identifierConstraints = expanded.Snapshot.Element.Where(e => e.Path.StartsWith("Patient.identifier"));

            dumpElements(identifierConstraints, "Constraints on Patient.identifier:");

            var nationalPatientProfile = resolver.FindStructureDefinition(profile.BaseDefinition);
            Assert.IsNotNull(nationalPatientProfile);
            Assert.IsTrue(nationalPatientProfile.HasSnapshot);
            var nationalPatientIdentifierElem = nationalPatientProfile.Snapshot.Element.FirstOrDefault(e => e.Path == "Patient.identifier");
            Assert.IsNotNull(nationalPatientIdentifierElem);
            Debug.Print($"Base: #{nationalPatientIdentifierElem.GetHashCode()} '{nationalPatientIdentifierElem.Path}'");

            dumpBaseElems(identifierConstraints);

            var nav = ElementDefinitionNavigator.ForSnapshot(expanded);
            Assert.IsTrue(nav.MoveToFirstChild());

            // Verify slice entry
            Assert.IsTrue(nav.MoveToChild("identifier"));
            Assert.AreEqual(nationalPatientIdentifierElem, GetBaseElementAnnotation(nav.Current));
            Assert.IsNotNull(nav.Current.Slicing);
            Assert.IsNull(nav.Current.SliceName);
            Assert.AreEqual(1, nav.Current.Min);
            Assert.AreEqual("*", nav.Current.Max);
            // Slice entry should inherit Comments from base element, merged with diff constraints
            Assert.AreEqual("NationalPatientProfile\r\nSlicedNationalPatientProfile", nav.Current.Comment);
            // Slice entry should also inherit constraints on child elements from base element
            var bm = nav.Bookmark();
            Assert.IsTrue(nav.MoveToChild("system"));
            Assert.AreEqual(nav.Current.Min, 1);
            Assert.IsTrue(nav.ReturnToBookmark(bm));

            // Verify slice "bsn"
            Assert.IsTrue(nav.MoveToNextSlice());
            Assert.AreEqual(nationalPatientIdentifierElem, GetBaseElementAnnotation(nav.Current));
            Assert.IsNull(nav.Current.Slicing);
            Assert.AreEqual("bsn", nav.Current.SliceName);
            Assert.AreEqual(1, nav.Current.Min);
            Assert.AreEqual("1", nav.Current.Max);
            // Named slices should inherit Comments from base element
            Assert.AreEqual("NationalPatientProfile", nav.Current.Comment);
            // Named slices should also inherit constraints on child elements from base element
            bm = nav.Bookmark();
            Assert.IsTrue(nav.MoveToChild("system"));
            Assert.AreEqual(nav.Current.Min, 1);
            // Should be merged with diff constraints on child elements
            Assert.AreEqual((nav.Current.Fixed as FhirUri).Value, "http://example.org/fhir/ValueSet/bsn");
            Assert.IsTrue(nav.ReturnToBookmark(bm));

            // Verify slice "ehr_id"
            Assert.IsTrue(nav.MoveToNextSlice());
            Assert.AreEqual(nationalPatientIdentifierElem, GetBaseElementAnnotation(nav.Current));
            Assert.IsNull(nav.Current.Slicing);
            Assert.AreEqual("ehr_id", nav.Current.SliceName);
            Assert.AreEqual(0, nav.Current.Min);
            Assert.AreEqual("2", nav.Current.Max);
            // Named slices should inherit Comments from base element
            Assert.AreEqual("NationalPatientProfile", nav.Current.Comment);
            // Named slices should also inherit constraints on child elements from base element
            bm = nav.Bookmark();
            Assert.IsTrue(nav.MoveToChild("system"));
            Assert.AreEqual(nav.Current.Min, 1);
            Assert.IsTrue(nav.ReturnToBookmark(bm));

#if false
            // Verify re-slice "ehr_id/temp"
            Assert.IsTrue(nav.MoveToNextSliceAtAnyLevel());
            Assert.AreEqual(nationalPatientIdentifierElem, GetBaseElementAnnotation(nav.Current));
            Assert.IsNull(nav.Current.Slicing);
            Assert.AreEqual("ehr_id/temp", nav.Current.SliceName);
            Assert.AreEqual(0, nav.Current.Min);
            Assert.AreEqual("1", nav.Current.Max);
            // Named slices should inherit Comments from base element
            Assert.AreEqual("NationalPatientProfile", nav.Current.Comment);
            // Named slices should also inherit constraints on child elements from base element
            bm = nav.Bookmark();
            Assert.IsTrue(nav.MoveToChild("system"));
            Assert.AreEqual(nav.Current.Min, 1);
            Assert.IsTrue(nav.ReturnToBookmark(bm));
#endif
        }

        static StructureDefinition ReslicedNationalPatientProfile => new StructureDefinition()
        {
            Type = FHIRAllTypes.Patient.GetLiteral(),
            BaseDefinition = "http://example.org/fhir/StructureDefinition/MyNationalPatient",
            Name = "ReslicedNationalPatientProfile",
            Url = "http://example.org/fhir/StructureDefinition/ReslicedNationalPatientProfile",
            Derivation = StructureDefinition.TypeDerivationRule.Constraint,
            Differential = new StructureDefinition.DifferentialComponent()
            {
                Element = new List<ElementDefinition>()
                {
                    new ElementDefinition("Patient.identifier")
                    {
                        Slicing = new ElementDefinition.SlicingComponent()
                        {
                            Discriminator = new ElementDefinition.DiscriminatorComponent[] 
                                { new ElementDefinition.DiscriminatorComponent
                                    { Type = ElementDefinition.DiscriminatorType.Value, Path = "system" }
                                }.ToList(),
                            Ordered = false,
                            Rules = ElementDefinition.SlicingRules.Open
                        },
                        Min = 1,
                        // Append to comment inherited from base
                        Comment = "...SlicedNationalPatientProfile"
                    }
                    // Slice: bsn
                    ,new ElementDefinition("Patient.identifier")
                    {
                        SliceName = "bsn",
                        Min = 1,
                        Max = "1"
                    },
                    new ElementDefinition("Patient.identifier.system")
                    {
                        Fixed = new FhirUri("http://example.org/fhir/ValueSet/bsn")
                    },
                    // Slice: ehr_id
                    new ElementDefinition("Patient.identifier")
                    {
                        SliceName = "ehr_id",
                        Max = "2",

                        // Re-slice the ehr-id
                        Slicing = new ElementDefinition.SlicingComponent()
                        {
                            // Discriminator = new string[] { "use" },
                            Discriminator = new ElementDefinition.DiscriminatorComponent[]
                                { new ElementDefinition.DiscriminatorComponent
                                    { Type = ElementDefinition.DiscriminatorType.Value, Path = "use" }
                                }.ToList(),
                            Ordered = true,
                            Rules = ElementDefinition.SlicingRules.Closed
                        }
                    },

                    // Reslice: ehr-id/temp
                    new ElementDefinition("Patient.identifier")
                    {
                        SliceName = "ehr_id/temp",
                        Max = "1",
                    },
                    new ElementDefinition("Patient.identifier.use")
                    {
                        // Fixed = new Code<Identifier.IdentifierUse>(Identifier.IdentifierUse.Temp)
                        Fixed = new Code("temp")
                    }
                }
            }
        };

        [TestMethod]
        public void TestSliceBase_ReslicedNationalPatient()
        {
            var baseProfile = NationalPatientProfile;
            var profile = ReslicedNationalPatientProfile;

            var resolver = new InMemoryProfileResolver(baseProfile, profile);
            var multiResolver = new MultiResolver(_testResolver, resolver);
            _generator = new SnapshotGenerator(multiResolver);
            StructureDefinition expanded = null;

            _generator.PrepareElement += elementHandler;
            try
            {
                generateSnapshotAndCompare(profile, out expanded);
            }
            finally
            {
                _generator.PrepareElement -= elementHandler;
            }
            dumpOutcome(_generator.Outcome);

            Assert.IsNotNull(expanded);
            Assert.IsTrue(expanded.HasSnapshot);

            var identifierConstraints = expanded.Snapshot.Element.Where(e => e.Path.StartsWith("Patient.identifier"));

            dumpElements(identifierConstraints, "Constraints on Patient.identifier:");

            var nationalPatientProfile = resolver.FindStructureDefinition(profile.BaseDefinition);
            Assert.IsNotNull(nationalPatientProfile);
            Assert.IsTrue(nationalPatientProfile.HasSnapshot);
            var nationalPatientIdentifierElem = nationalPatientProfile.Snapshot.Element.FirstOrDefault(e => e.Path == "Patient.identifier");
            Assert.IsNotNull(nationalPatientIdentifierElem);
            Debug.Print($"Base: #{nationalPatientIdentifierElem.GetHashCode()} '{nationalPatientIdentifierElem.Path}'");

            dumpBaseElems(identifierConstraints);

            var nav = ElementDefinitionNavigator.ForSnapshot(expanded);
            Assert.IsTrue(nav.MoveToFirstChild());

            // Verify slice entry
            Assert.IsTrue(nav.MoveToChild("identifier"));
            Assert.AreEqual(nationalPatientIdentifierElem, GetBaseElementAnnotation(nav.Current));
            Assert.IsNotNull(nav.Current.Slicing);
            Assert.IsNull(nav.Current.SliceName);
            Assert.AreEqual(1, nav.Current.Min);
            Assert.AreEqual("*", nav.Current.Max);
            // Slice entry should inherit Comments from base element, merged with diff constraints
            Assert.AreEqual("NationalPatientProfile\r\nSlicedNationalPatientProfile", nav.Current.Comment);
            // Slice entry should also inherit constraints on child elements from base element
            var bm = nav.Bookmark();
            Assert.IsTrue(nav.MoveToChild("system"));
            Assert.AreEqual(nav.Current.Min, 1);
            Assert.IsTrue(nav.ReturnToBookmark(bm));

            // Verify slice "bsn"
            Assert.IsTrue(nav.MoveToNextSlice());
            Assert.AreEqual(nationalPatientIdentifierElem, GetBaseElementAnnotation(nav.Current));
            Assert.IsNull(nav.Current.Slicing);
            Assert.AreEqual("bsn", nav.Current.SliceName);
            Assert.AreEqual(1, nav.Current.Min);
            Assert.AreEqual("1", nav.Current.Max);
            // Named slices should inherit Comments from base element
            Assert.AreEqual("NationalPatientProfile", nav.Current.Comment);
            // Named slices should also inherit constraints on child elements from base element
            bm = nav.Bookmark();
            Assert.IsTrue(nav.MoveToChild("system"));
            Assert.AreEqual(nav.Current.Min, 1);
            // Should be merged with diff constraints on child elements
            Assert.AreEqual((nav.Current.Fixed as FhirUri).Value, "http://example.org/fhir/ValueSet/bsn");
            Assert.IsTrue(nav.ReturnToBookmark(bm));

            // Verify slice "ehr_id"
            Assert.IsTrue(nav.MoveToNextSlice());
            Assert.AreEqual(nationalPatientIdentifierElem, GetBaseElementAnnotation(nav.Current));
            Assert.IsNotNull(nav.Current.Slicing);
            Assert.AreEqual("ehr_id", nav.Current.SliceName);
            Assert.AreEqual(0, nav.Current.Min);
            Assert.AreEqual("2", nav.Current.Max);
            // Named slices should inherit Comments from base element
            Assert.AreEqual("NationalPatientProfile", nav.Current.Comment);
            // Named slices should also inherit constraints on child elements from base element
            bm = nav.Bookmark();
            Assert.IsTrue(nav.MoveToChild("system"));
            Assert.AreEqual(nav.Current.Min, 1);
            Assert.IsTrue(nav.ReturnToBookmark(bm));

            // Verify re-slice "ehr_id/temp"
            Assert.IsTrue(nav.MoveToFirstReslice());
            Assert.AreEqual(nationalPatientIdentifierElem, GetBaseElementAnnotation(nav.Current));
            Assert.IsNull(nav.Current.Slicing);
            Assert.AreEqual("ehr_id/temp", nav.Current.SliceName);
            Assert.AreEqual(0, nav.Current.Min);
            Assert.AreEqual("1", nav.Current.Max);
            // Named slices should inherit Comments from base element
            Assert.AreEqual("NationalPatientProfile", nav.Current.Comment);
            // Named slices should also inherit constraints on child elements from base element
            bm = nav.Bookmark();
            Assert.IsTrue(nav.MoveToChild("system"));
            Assert.AreEqual(nav.Current.Min, 1);
            Assert.IsTrue(nav.ReturnToBookmark(bm));
        }

        [TestMethod]
        public void TestSliceBase_PatientTelecomResliceEK()
        {
            var dirSource = new DirectorySource("TestData/validation", false);
            var source = new TimingSource(dirSource);
            var resolver = new CachedResolver(source);
            var multiResolver = new MultiResolver(resolver, _testResolver);

            var profile = resolver.FindStructureDefinition("http://example.com/StructureDefinition/patient-telecom-reslice-ek");
            Assert.IsNotNull(profile);

            var settings = new SnapshotGeneratorSettings(_settings);
            settings.GenerateElementIds = true;
            _generator = new SnapshotGenerator(multiResolver, settings);
            StructureDefinition expanded = null;

            _generator.PrepareElement += elementHandler;
            try
            {
                generateSnapshotAndCompare(profile, out expanded);
            }
            finally
            {
                _generator.PrepareElement -= elementHandler;
            }
            dumpOutcome(_generator.Outcome);

            Assert.IsNotNull(expanded);
            Assert.IsTrue(expanded.HasSnapshot);

            dumpElements(expanded.Snapshot.Element);

            var nav = ElementDefinitionNavigator.ForSnapshot(expanded);
            Assert.IsTrue(nav.MoveToFirstChild());

            // Patient.telecom slice entry
            Assert.IsTrue(nav.MoveToChild("telecom"));
            Assert.IsNotNull(nav.Current.Slicing);
            Assert.AreEqual(true, nav.Current.Slicing.Ordered);
            Assert.AreEqual(ElementDefinition.SlicingRules.OpenAtEnd, nav.Current.Slicing.Rules);
            Assert.IsFalse(nav.Current.Slicing.Discriminator.Any());
            Assert.AreEqual(1, nav.Current.Min);
            Assert.AreEqual("5", nav.Current.Max);

            // Patient.telecom:phone
            Assert.IsTrue(nav.MoveToNext("telecom"));
            Assert.AreEqual("phone", nav.Current.SliceName);
            Assert.AreEqual(1, nav.Current.Min);
            Assert.AreEqual("2", nav.Current.Max);
            Assert.IsNull(nav.Current.Slicing);

            // Patient.telecom.system
            var bm = nav.Bookmark();
            Assert.IsTrue(nav.MoveToChild("system"));
            Assert.AreEqual(1, nav.Current.Min);
            Assert.AreEqual("phone", (nav.Current.Fixed as Code)?.Value);
            Assert.IsTrue(nav.ReturnToBookmark(bm));

            // Patient.telecom:email
            Assert.IsTrue(nav.MoveToNext("telecom"));
            Assert.AreEqual("email", nav.Current.SliceName);
            Assert.AreEqual(0, nav.Current.Min);
            Assert.AreEqual("1", nav.Current.Max);
            Assert.IsNotNull(nav.Current.Slicing);
            // TODO: BRIAN: Need to check that this is the correct assertion here
            Assert.AreEqual("system|use", string.Join("|", nav.Current.Slicing.Discriminator.Select(s => s.Path)));
            // Assert.AreEqual(1, nav.Current.Slicing.Discriminator.SelectMany(s => s.Type.Value).Count()));
            Assert.AreEqual(ElementDefinition.SlicingRules.Closed, nav.Current.Slicing.Rules);
            // Assert.AreEqual(false, nav.Current.Slicing.Ordered);
            Assert.IsNull(nav.Current.Slicing.Ordered);

            // Patient.telecom.system
            bm = nav.Bookmark();
            Assert.IsTrue(nav.MoveToChild("system"));
            Assert.AreEqual(1, nav.Current.Min);
            Assert.AreEqual("email", (nav.Current.Fixed as Code)?.Value);
            Assert.IsTrue(nav.ReturnToBookmark(bm));

            // Patient.telecom:email/home
            Assert.IsTrue(nav.MoveToNext("telecom"));
            Assert.AreEqual("email/home", nav.Current.SliceName);
            Assert.AreEqual(0, nav.Current.Min);
            Assert.AreEqual("1", nav.Current.Max);
            Assert.IsNull(nav.Current.Slicing);

            // Patient.telecom.system
            bm = nav.Bookmark();
            Assert.IsTrue(nav.MoveToChild("system"));
            Assert.AreEqual(1, nav.Current.Min);
            Assert.AreEqual("email", (nav.Current.Fixed as Code)?.Value);
            Assert.IsTrue(nav.MoveToNext("use"));
            Assert.AreEqual(1, nav.Current.Min);
            Assert.AreEqual("home", (nav.Current.Fixed as Code)?.Value);
            Assert.IsTrue(nav.ReturnToBookmark(bm));

            // Patient.telecom:email/work
            Assert.IsTrue(nav.MoveToNext("telecom"));
            Assert.AreEqual("email/work", nav.Current.SliceName);
            Assert.AreEqual(0, nav.Current.Min);
            Assert.AreEqual("1", nav.Current.Max);
            Assert.IsNull(nav.Current.Slicing);

            // Patient.telecom.system
            bm = nav.Bookmark();
            Assert.IsTrue(nav.MoveToChild("system"));
            Assert.AreEqual(1, nav.Current.Min);
            Assert.AreEqual("email", (nav.Current.Fixed as Code)?.Value);
            Assert.IsTrue(nav.MoveToNext("use"));
            Assert.AreEqual(1, nav.Current.Min);
            Assert.AreEqual("work", (nav.Current.Fixed as Code)?.Value);
            Assert.IsTrue(nav.ReturnToBookmark(bm));

            // Patient.telecom:other
            Assert.IsTrue(nav.MoveToNext("telecom"));
            Assert.AreEqual("other", nav.Current.SliceName);
            Assert.AreEqual(0, nav.Current.Min);
            Assert.AreEqual("3", nav.Current.Max);
            Assert.IsNotNull(nav.Current.Slicing);
            Assert.AreEqual("system|use", string.Join("|", nav.Current.Slicing.Discriminator.Select(p => p.Path)));
            Assert.AreEqual(ElementDefinition.SlicingRules.Open, nav.Current.Slicing.Rules);
            // Assert.AreEqual(false, nav.Current.Slicing.Ordered);
            Assert.IsNull(nav.Current.Slicing.Ordered);

            // Patient.telecom.system
            bm = nav.Bookmark();
            Assert.IsTrue(nav.MoveToChild("system"));
            Assert.AreEqual(1, nav.Current.Min);
            Assert.AreEqual("other", (nav.Current.Fixed as Code)?.Value);
            Assert.IsTrue(nav.ReturnToBookmark(bm));

            // Patient.telecom:other/home
            Assert.IsTrue(nav.MoveToNext("telecom"));
            Assert.AreEqual("other/home", nav.Current.SliceName);
            Assert.AreEqual(0, nav.Current.Min);
            Assert.AreEqual("1", nav.Current.Max);
            Assert.IsNull(nav.Current.Slicing);

            // Patient.telecom.system
            bm = nav.Bookmark();
            Assert.IsTrue(nav.MoveToChild("system"));
            Assert.AreEqual(1, nav.Current.Min);
            Assert.AreEqual("other", (nav.Current.Fixed as Code)?.Value);
            Assert.IsTrue(nav.MoveToNext("use"));
            Assert.AreEqual(1, nav.Current.Min);
            Assert.AreEqual("home", (nav.Current.Fixed as Code)?.Value);
            Assert.IsTrue(nav.ReturnToBookmark(bm));

            // Patient.telecom:other/work
            Assert.IsTrue(nav.MoveToNext("telecom"));
            Assert.AreEqual("other/work", nav.Current.SliceName);
            Assert.AreEqual(0, nav.Current.Min);
            Assert.AreEqual("1", nav.Current.Max);
            Assert.IsNull(nav.Current.Slicing);

            // Patient.telecom.system
            bm = nav.Bookmark();
            Assert.IsTrue(nav.MoveToChild("system"));
            Assert.AreEqual(1, nav.Current.Min);
            Assert.AreEqual("other", (nav.Current.Fixed as Code)?.Value);
            Assert.IsTrue(nav.MoveToNext("use"));
            Assert.AreEqual(1, nav.Current.Min);
            Assert.AreEqual("work", (nav.Current.Fixed as Code)?.Value);
            Assert.IsTrue(nav.ReturnToBookmark(bm));
        }

        [TestMethod]
        public void TestElementMappings()
        {
            var profile = _testResolver.FindStructureDefinition("http://example.org/fhir/StructureDefinition/TestMedicationStatement-prescribing");
            Assert.IsNotNull(profile);

            var diffElem = profile.Differential.Element.FirstOrDefault(e => e.Path == "MedicationStatement.informationSource");
            Assert.IsNotNull(diffElem);
            dumpMappings(diffElem);

            StructureDefinition expanded = null;
            _generator = new SnapshotGenerator(_testResolver, _settings);
            _generator.PrepareElement += elementHandler;
            try
            {
                generateSnapshotAndCompare(profile, out expanded);
            }
            finally
            {
                _generator.PrepareElement -= elementHandler;
            }
            dumpOutcome(_generator.Outcome);

            Assert.IsNotNull(expanded);
            Assert.IsTrue(expanded.HasSnapshot);

            var elems = expanded.Snapshot.Element;
            dumpElements(elems);

            var elem = elems.FirstOrDefault(e => e.Path == "MedicationStatement.informationSource");
            Assert.IsNotNull(elem);
            dumpMappings(elem);

            // Snapshot element mappings should include all of the differential element mappings
            Assert.IsTrue(diffElem.Mapping.All(dm => elem.Mapping.Any(m => m.IsExactly(dm))));

        }

        static void dumpMappings(ElementDefinition elem) => dumpMappings(elem.Mapping, $"Mappings for {elem.Path}:");

        static void dumpMappings(IList<ElementDefinition.MappingComponent> mappings, string header = null)
        {
            Debug.WriteLineIf(header != null, header);
            foreach (var mapping in mappings)
            {
                Debug.Print($"{mapping.Identity} : {mapping.Map}");
            }
        }

        // Ewout: type slices cannot contain renamed elements!

        static StructureDefinition PatientNonTypeSliceProfile => new StructureDefinition()
        {
            Type = FHIRAllTypes.Patient.GetLiteral(),
            BaseDefinition = ModelInfo.CanonicalUriForFhirCoreType(FHIRAllTypes.Patient),
            Name = "NonTypeSlicePatient",
            Url = "http://example.org/fhir/StructureDefinition/NonTypeSlicePatient",
            Differential = new StructureDefinition.DifferentialComponent()
            {
                Element = new List<ElementDefinition>()
                {
                    new ElementDefinition("Patient.deceased[x]")
                    {
                        Min = 1,
                        // Repeat the base element types (no additional constraints)
                        Type = new List<ElementDefinition.TypeRefComponent>()
                        {
                            new ElementDefinition.TypeRefComponent() { Code = FHIRAllTypes.Boolean.GetLiteral() },
                            new ElementDefinition.TypeRefComponent() { Code = FHIRAllTypes.DateTime.GetLiteral() }
                        }
                    }
                }
            }
        };

        [TestMethod]
        public void TestPatientNonTypeSlice()
        {
            var profile = PatientNonTypeSliceProfile;

            var resolver = new InMemoryProfileResolver(profile);
            var multiResolver = new MultiResolver(_testResolver, resolver);
            _generator = new SnapshotGenerator(multiResolver);

            //StructureDefinition expanded = null;
            //generateSnapshotAndCompare(profile, out expanded);

            //_generator.BeforeExpandElement += beforeExpandElementHandler;
            //StructureDefinition expanded = null;
            //try
            //{
            //    generateSnapshotAndCompare(profile, out expanded);
            //}
            //finally
            //{
            //    _generator.BeforeExpandElement -= beforeExpandElementHandler;
            //}
            //Assert.IsNotNull(expanded);
            //Assert.IsTrue(expanded.HasSnapshot);
            //dumpElements(expanded.Snapshot.Element);
            //dumpOutcome(_generator.Outcome);

            // Force expansion of Patient.deceased[x]
            var nav = ElementDefinitionNavigator.ForDifferential(profile);
            Assert.IsTrue(nav.MoveToFirstChild());
            var result = _generator.ExpandElement(nav);
            dumpElements(profile.Differential.Element);
            dumpOutcome(_generator.Outcome);
            Assert.IsTrue(result);

            Assert.IsNull(_generator.Outcome);
        }

        // Ewout: type slices cannot contain renamed elements!
        static StructureDefinition ObservationSimpleQuantityProfile => new StructureDefinition()
        {
            Type = FHIRAllTypes.Observation.GetLiteral(),
            BaseDefinition = ModelInfo.CanonicalUriForFhirCoreType(FHIRAllTypes.Observation),
            Name = "NonTypeSlicePatient",
            Url = "http://example.org/fhir/StructureDefinition/ObservationSimpleQuantityProfile",
            Differential = new StructureDefinition.DifferentialComponent()
            {
                Element = new List<ElementDefinition>()
                {
                    new ElementDefinition("Observation.valueQuantity")
                    {
                        // Repeat the base element types (no additional constraints)
                        Type = new List<ElementDefinition.TypeRefComponent>()
                        {
                            new ElementDefinition.TypeRefComponent()
                            {
                                // Constrain Quantity to SimpleQuantity
                                // Code = FHIRDefinedType.Quantity,
                                // Profile = new string[] { ModelInfo.CanonicalUriForFhirCoreType(FHIRDefinedType.SimpleQuantity) }

                                Code = FHIRAllTypes.SimpleQuantity.GetLiteral()
                            },
                        }
                    }
                }
            }
        };

        // [WMR 20170321] NEW
        [TestMethod]
        public void TestSimpleQuantityProfile()
        {
            var profile = ObservationSimpleQuantityProfile;

            var resolver = new InMemoryProfileResolver(profile);
            var multiResolver = new MultiResolver(_testResolver, resolver);
            _generator = new SnapshotGenerator(multiResolver);

            _generator.BeforeExpandElement += beforeExpandElementHandler;
            StructureDefinition expanded = null;
            try
            {
                generateSnapshotAndCompare(profile, out expanded);
            }
            finally
            {
                _generator.BeforeExpandElement -= beforeExpandElementHandler;
            }
            Assert.IsNotNull(expanded);
            Assert.IsTrue(expanded.HasSnapshot);
            dumpElements(expanded.Snapshot.Element.Where(e => e.Path.StartsWith("Observation.value")));
            dumpOutcome(_generator.Outcome);

            // Force expansion of Observation.valueQuantity
            //var nav = ElementDefinitionNavigator.ForDifferential(profile);
            //Assert.IsTrue(nav.MoveToFirstChild());
            //var result = _generator.ExpandElement(nav);
            //dumpElements(profile.Differential.Element);
            //dumpOutcome(_generator.Outcome);
            //Assert.IsTrue(result);
            Assert.IsNull(_generator.Outcome);

            // Ensure that renamed diff elements override base elements with original names
            var nav = ElementDefinitionNavigator.ForSnapshot(expanded);
            // Snapshot should not contain elements with original name
            Assert.IsFalse(nav.JumpToFirst("Observation.value[x]"));
            // Snapshot should contain renamed elements
            Assert.IsTrue(nav.JumpToFirst("Observation.valueQuantity"));
            Assert.IsNotNull(nav.Current.Type);
            Assert.AreEqual(1, nav.Current.Type.Count);
            // Assert.AreEqual(FHIRDefinedType.SimpleQuantity, nav.Current.Type[0].Code);
            // Assert.AreEqual(FHIRDefinedType.Quantity, nav.Current.Type[0].Code);

            var type = nav.Current.Type.First();
            Debug.Print($"{nav.Path} : {type.Code} - '{type.Profile.FirstOrDefault()}'");
        }
    }

}
