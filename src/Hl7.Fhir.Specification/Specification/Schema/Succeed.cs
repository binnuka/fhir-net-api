﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Hl7.Fhir.ElementModel;
using Hl7.Fhir.Specification.Schema.Tags;

namespace Hl7.Fhir.Specification.Schema
{
    public class Succeed : Assertion, IMemberAssertion
    {
        public override IEnumerable<SchemaTags> CollectTags() => SchemaTags.Success.Collection;

        public override IEnumerable<Assertions> CollectAssertions(Predicate<Assertion> pred)
            => pred(this) ? new Assertions(this).Collection : Assertions.Empty.Collection;
        
        public SchemaTags Validate(IElementNavigator input, ValidationContext vc)
            => SchemaTags.Success;
    }
}