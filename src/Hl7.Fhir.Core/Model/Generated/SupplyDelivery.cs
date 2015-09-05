﻿using System;
using System.Collections.Generic;
using Hl7.Fhir.Introspection;
using Hl7.Fhir.Validation;
using System.Linq;
using System.Runtime.Serialization;

/*
  Copyright (c) 2011+, HL7, Inc.
  All rights reserved.
  
  Redistribution and use in source and binary forms, with or without modification, 
  are permitted provided that the following conditions are met:
  
   * Redistributions of source code must retain the above copyright notice, this 
     list of conditions and the following disclaimer.
   * Redistributions in binary form must reproduce the above copyright notice, 
     this list of conditions and the following disclaimer in the documentation 
     and/or other materials provided with the distribution.
   * Neither the name of HL7 nor the names of its contributors may be used to 
     endorse or promote products derived from this software without specific 
     prior written permission.
  
  THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND 
  ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED 
  WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED. 
  IN NO EVENT SHALL THE COPYRIGHT HOLDER OR CONTRIBUTORS BE LIABLE FOR ANY DIRECT, 
  INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT 
  NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR 
  PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, 
  WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) 
  ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF THE 
  POSSIBILITY OF SUCH DAMAGE.
  

*/

//
// Generated on Tue, Sep 1, 2015 21:04+1000 for FHIR v1.0.0
//
namespace Hl7.Fhir.Model
{
    /// <summary>
    /// Delivery of Supply
    /// </summary>
    [FhirType("SupplyDelivery", IsResource=true)]
    [DataContract]
    public partial class SupplyDelivery : Hl7.Fhir.Model.DomainResource, System.ComponentModel.INotifyPropertyChanged
    {
        [NotMapped]
        public override ResourceType ResourceType { get { return ResourceType.SupplyDelivery; } }
        [NotMapped]
        public override string TypeName { get { return "SupplyDelivery"; } }
        
        /// <summary>
        /// Status of the Supply delivery
        /// </summary>
        [FhirEnumeration("SupplyDeliveryStatus")]
        public enum SupplyDeliveryStatus
        {
            /// <summary>
            /// Supply has been requested, but not delivered
            /// </summary>
            [EnumLiteral("in-progress")]
            InProgress,
            /// <summary>
            /// Supply has been delivered. ( "completed")
            /// </summary>
            [EnumLiteral("completed")]
            Completed,
            /// <summary>
            /// Dispensing was not completed
            /// </summary>
            [EnumLiteral("abandoned")]
            Abandoned,
        }
        
        /// <summary>
        /// External identifier
        /// </summary>
        [FhirElement("identifier", Order=90)]
        [DataMember]
        public Hl7.Fhir.Model.Identifier Identifier
        {
            get { return _Identifier; }
            set { _Identifier = value; OnPropertyChanged("Identifier"); }
        }
        
        private Hl7.Fhir.Model.Identifier _Identifier;
        
        /// <summary>
        /// in-progress | completed | abandoned
        /// </summary>
        [FhirElement("status", Order=100)]
        [DataMember]
        public Code<Hl7.Fhir.Model.SupplyDelivery.SupplyDeliveryStatus> StatusElement
        {
            get { return _StatusElement; }
            set { _StatusElement = value; OnPropertyChanged("StatusElement"); }
        }
        
        private Code<Hl7.Fhir.Model.SupplyDelivery.SupplyDeliveryStatus> _StatusElement;
        
        /// <summary>
        /// in-progress | completed | abandoned
        /// </summary>
        /// <remarks>This uses the native .NET datatype, rather than the FHIR equivalent</remarks>
        [NotMapped]
        [IgnoreDataMemberAttribute]
        public Hl7.Fhir.Model.SupplyDelivery.SupplyDeliveryStatus? Status
        {
            get { return StatusElement != null ? StatusElement.Value : null; }
            set
            {
                if(value == null)
                  StatusElement = null; 
                else
                  StatusElement = new Code<Hl7.Fhir.Model.SupplyDelivery.SupplyDeliveryStatus>(value);
                OnPropertyChanged("Status");
            }
        }
        
        /// <summary>
        /// Patient for whom the item is supplied
        /// </summary>
        [FhirElement("patient", Order=110)]
        [References("Patient")]
        [DataMember]
        public Hl7.Fhir.Model.ResourceReference Patient
        {
            get { return _Patient; }
            set { _Patient = value; OnPropertyChanged("Patient"); }
        }
        
        private Hl7.Fhir.Model.ResourceReference _Patient;
        
        /// <summary>
        /// Category of dispense event
        /// </summary>
        [FhirElement("type", Order=120)]
        [DataMember]
        public Hl7.Fhir.Model.CodeableConcept Type
        {
            get { return _Type; }
            set { _Type = value; OnPropertyChanged("Type"); }
        }
        
        private Hl7.Fhir.Model.CodeableConcept _Type;
        
        /// <summary>
        /// Amount dispensed
        /// </summary>
        [FhirElement("quantity", Order=130)]
        [DataMember]
        public Hl7.Fhir.Model.SimpleQuantity Quantity
        {
            get { return _Quantity; }
            set { _Quantity = value; OnPropertyChanged("Quantity"); }
        }
        
        private Hl7.Fhir.Model.SimpleQuantity _Quantity;
        
        /// <summary>
        /// Medication, Substance, or Device supplied
        /// </summary>
        [FhirElement("suppliedItem", Order=140)]
        [References("Medication","Substance","Device")]
        [DataMember]
        public Hl7.Fhir.Model.ResourceReference SuppliedItem
        {
            get { return _SuppliedItem; }
            set { _SuppliedItem = value; OnPropertyChanged("SuppliedItem"); }
        }
        
        private Hl7.Fhir.Model.ResourceReference _SuppliedItem;
        
        /// <summary>
        /// Dispenser
        /// </summary>
        [FhirElement("supplier", Order=150)]
        [References("Practitioner")]
        [DataMember]
        public Hl7.Fhir.Model.ResourceReference Supplier
        {
            get { return _Supplier; }
            set { _Supplier = value; OnPropertyChanged("Supplier"); }
        }
        
        private Hl7.Fhir.Model.ResourceReference _Supplier;
        
        /// <summary>
        /// Dispensing time
        /// </summary>
        [FhirElement("whenPrepared", Order=160)]
        [DataMember]
        public Hl7.Fhir.Model.Period WhenPrepared
        {
            get { return _WhenPrepared; }
            set { _WhenPrepared = value; OnPropertyChanged("WhenPrepared"); }
        }
        
        private Hl7.Fhir.Model.Period _WhenPrepared;
        
        /// <summary>
        /// Handover time
        /// </summary>
        [FhirElement("time", Order=170)]
        [DataMember]
        public Hl7.Fhir.Model.FhirDateTime TimeElement
        {
            get { return _TimeElement; }
            set { _TimeElement = value; OnPropertyChanged("TimeElement"); }
        }
        
        private Hl7.Fhir.Model.FhirDateTime _TimeElement;
        
        /// <summary>
        /// Handover time
        /// </summary>
        /// <remarks>This uses the native .NET datatype, rather than the FHIR equivalent</remarks>
        [NotMapped]
        [IgnoreDataMemberAttribute]
        public string Time
        {
            get { return TimeElement != null ? TimeElement.Value : null; }
            set
            {
                if(value == null)
                  TimeElement = null; 
                else
                  TimeElement = new Hl7.Fhir.Model.FhirDateTime(value);
                OnPropertyChanged("Time");
            }
        }
        
        /// <summary>
        /// Where the Supply was sent
        /// </summary>
        [FhirElement("destination", Order=180)]
        [References("Location")]
        [DataMember]
        public Hl7.Fhir.Model.ResourceReference Destination
        {
            get { return _Destination; }
            set { _Destination = value; OnPropertyChanged("Destination"); }
        }
        
        private Hl7.Fhir.Model.ResourceReference _Destination;
        
        /// <summary>
        /// Who collected the Supply
        /// </summary>
        [FhirElement("receiver", Order=190)]
        [References("Practitioner")]
        [Cardinality(Min=0,Max=-1)]
        [DataMember]
        public List<Hl7.Fhir.Model.ResourceReference> Receiver
        {
            get { if(_Receiver==null) _Receiver = new List<Hl7.Fhir.Model.ResourceReference>(); return _Receiver; }
            set { _Receiver = value; OnPropertyChanged("Receiver"); }
        }
        
        private List<Hl7.Fhir.Model.ResourceReference> _Receiver;
        
        public override IDeepCopyable CopyTo(IDeepCopyable other)
        {
            var dest = other as SupplyDelivery;
            
            if (dest != null)
            {
                base.CopyTo(dest);
                if(Identifier != null) dest.Identifier = (Hl7.Fhir.Model.Identifier)Identifier.DeepCopy();
                if(StatusElement != null) dest.StatusElement = (Code<Hl7.Fhir.Model.SupplyDelivery.SupplyDeliveryStatus>)StatusElement.DeepCopy();
                if(Patient != null) dest.Patient = (Hl7.Fhir.Model.ResourceReference)Patient.DeepCopy();
                if(Type != null) dest.Type = (Hl7.Fhir.Model.CodeableConcept)Type.DeepCopy();
                if(Quantity != null) dest.Quantity = (Hl7.Fhir.Model.SimpleQuantity)Quantity.DeepCopy();
                if(SuppliedItem != null) dest.SuppliedItem = (Hl7.Fhir.Model.ResourceReference)SuppliedItem.DeepCopy();
                if(Supplier != null) dest.Supplier = (Hl7.Fhir.Model.ResourceReference)Supplier.DeepCopy();
                if(WhenPrepared != null) dest.WhenPrepared = (Hl7.Fhir.Model.Period)WhenPrepared.DeepCopy();
                if(TimeElement != null) dest.TimeElement = (Hl7.Fhir.Model.FhirDateTime)TimeElement.DeepCopy();
                if(Destination != null) dest.Destination = (Hl7.Fhir.Model.ResourceReference)Destination.DeepCopy();
                if(Receiver != null) dest.Receiver = new List<Hl7.Fhir.Model.ResourceReference>(Receiver.DeepCopy());
                return dest;
            }
            else
            	throw new ArgumentException("Can only copy to an object of the same type", "other");
        }
        
        public override IDeepCopyable DeepCopy()
        {
            return CopyTo(new SupplyDelivery());
        }
        
        public override bool Matches(IDeepComparable other)
        {
            var otherT = other as SupplyDelivery;
            if(otherT == null) return false;
            
            if(!base.Matches(otherT)) return false;
            if( !DeepComparable.Matches(Identifier, otherT.Identifier)) return false;
            if( !DeepComparable.Matches(StatusElement, otherT.StatusElement)) return false;
            if( !DeepComparable.Matches(Patient, otherT.Patient)) return false;
            if( !DeepComparable.Matches(Type, otherT.Type)) return false;
            if( !DeepComparable.Matches(Quantity, otherT.Quantity)) return false;
            if( !DeepComparable.Matches(SuppliedItem, otherT.SuppliedItem)) return false;
            if( !DeepComparable.Matches(Supplier, otherT.Supplier)) return false;
            if( !DeepComparable.Matches(WhenPrepared, otherT.WhenPrepared)) return false;
            if( !DeepComparable.Matches(TimeElement, otherT.TimeElement)) return false;
            if( !DeepComparable.Matches(Destination, otherT.Destination)) return false;
            if( !DeepComparable.Matches(Receiver, otherT.Receiver)) return false;
            
            return true;
        }
        
        public override bool IsExactly(IDeepComparable other)
        {
            var otherT = other as SupplyDelivery;
            if(otherT == null) return false;
            
            if(!base.IsExactly(otherT)) return false;
            if( !DeepComparable.IsExactly(Identifier, otherT.Identifier)) return false;
            if( !DeepComparable.IsExactly(StatusElement, otherT.StatusElement)) return false;
            if( !DeepComparable.IsExactly(Patient, otherT.Patient)) return false;
            if( !DeepComparable.IsExactly(Type, otherT.Type)) return false;
            if( !DeepComparable.IsExactly(Quantity, otherT.Quantity)) return false;
            if( !DeepComparable.IsExactly(SuppliedItem, otherT.SuppliedItem)) return false;
            if( !DeepComparable.IsExactly(Supplier, otherT.Supplier)) return false;
            if( !DeepComparable.IsExactly(WhenPrepared, otherT.WhenPrepared)) return false;
            if( !DeepComparable.IsExactly(TimeElement, otherT.TimeElement)) return false;
            if( !DeepComparable.IsExactly(Destination, otherT.Destination)) return false;
            if( !DeepComparable.IsExactly(Receiver, otherT.Receiver)) return false;
            
            return true;
        }
        
    }
    
}
