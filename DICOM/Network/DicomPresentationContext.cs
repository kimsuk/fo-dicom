﻿// Copyright (c) 2012-2016 fo-dicom contributors.
// Licensed under the Microsoft Public License (MS-PL).

namespace Dicom.Network
{
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Linq;

    /// <summary>
    /// Enumeration of presentation context results.
    /// </summary>
    public enum DicomPresentationContextResult : byte
    {
        /// <summary>
        /// Presentation context is proposed.
        /// </summary>
        Proposed = 255,

        /// <summary>
        /// Presentation context is accepted.
        /// </summary>
        Accept = 0,

        /// <summary>
        /// Presentation context is rejected by user.
        /// </summary>
        RejectUser = 1,

        /// <summary>
        /// Presentation context is rejected for unspecified reason.
        /// </summary>
        RejectNoReason = 2,

        /// <summary>
        /// Presentation context is rejected due to abstract syntax not being supported.
        /// </summary>
        RejectAbstractSyntaxNotSupported = 3,

        /// <summary>
        /// Presentation context is rejected due to transfer syntaxes not being supported.
        /// </summary>
        RejectTransferSyntaxesNotSupported = 4
    }

    /// <summary>
    /// Representation of a presentation context.
    /// </summary>
    public class DicomPresentationContext
    {
        #region Private Members

        private readonly byte _pcid;

        private DicomPresentationContextResult _result;

        private readonly DicomUID _abstract;

        private readonly List<DicomTransferSyntax> _transferSyntaxes;

        #endregion

        #region Public Constructor

        /// <summary>
        /// Initializes a new instance of the <see cref="DicomPresentationContext"/> class.
        /// </summary>
        /// <param name="pcid">
        /// The presentation context ID.
        /// </param>
        /// <param name="abstractSyntax">
        /// The abstract syntax associated with the presentation context.
        /// </param>
        public DicomPresentationContext(byte pcid, DicomUID abstractSyntax)
            : this(pcid, abstractSyntax, null, null)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DicomPresentationContext"/> class.
        /// </summary>
        /// <param name="pcid">
        /// The presentation context ID.
        /// </param>
        /// <param name="abstractSyntax">
        /// The abstract syntax associated with the presentation context.
        /// </param>
        /// <param name="userRole">
        /// Indicates whether SCU role is supported.
        /// </param>
        /// <param name="providerRole">
        /// Indicates whether SCP role is supported.
        /// </param>
        public DicomPresentationContext(
            byte pcid,
            DicomUID abstractSyntax,
            bool? userRole,
            bool? providerRole)
        {
            _pcid = pcid;
            _result = DicomPresentationContextResult.Proposed;
            _abstract = abstractSyntax;
            _transferSyntaxes = new List<DicomTransferSyntax>();
            UserRole = userRole;
            ProviderRole = providerRole;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DicomPresentationContext"/> class.
        /// </summary>
        /// <param name="pcid">
        /// The presentation context ID.
        /// </param>
        /// <param name="abstractSyntax">
        /// The abstract syntax associated with the presentation context.
        /// </param>
        /// <param name="transferSyntax">
        /// Accepted transfer syntax.
        /// </param>
        /// <param name="result">
        /// Result of presentation context negotiation.
        /// </param>
        internal DicomPresentationContext(
            byte pcid,
            DicomUID abstractSyntax,
            DicomTransferSyntax transferSyntax,
            DicomPresentationContextResult result)
        {
            _pcid = pcid;
            _result = result;
            _abstract = abstractSyntax;
            _transferSyntaxes = new List<DicomTransferSyntax>();
            _transferSyntaxes.Add(transferSyntax);
            UserRole = null;
            ProviderRole = null;
        }

        #endregion

        #region Public Properties

        /// <summary>
        /// Gets the presentation context ID.
        /// </summary>
        public byte ID => _pcid;

        /// <summary>
        /// Gets the association negotiation result.
        /// </summary>
        public DicomPresentationContextResult Result =>_result;

        /// <summary>
        /// Gets the abstact syntax associated with the presentation context.
        /// </summary>
        public DicomUID AbstractSyntax => _abstract;

        /// <summary>
        /// Gets the accepted transfer syntax, if defined, otherwise <code>null</code>.
        /// </summary>
        public DicomTransferSyntax AcceptedTransferSyntax
        {
            get
            {
                if (_transferSyntaxes.Count > 0) return _transferSyntaxes[0];
                return null;
            }
        }

        /// <summary>
        /// Gets an indicator whether presentation context supports an SCU role. If undefined, default value is assumed.
        /// </summary>
        public bool? UserRole { get; set; }

        /// <summary>
        /// Gets an indicator whether presentation context supports an SCU role. If undefined, default value is assumed.
        /// </summary>
        public bool? ProviderRole { get; set; }

        #endregion

        #region Public Members

        /// <summary>
        /// Sets the <c>Result</c> of this presentation context.
        /// 
        /// The preferred method of accepting presentation contexts is to call one of the <c>AcceptTransferSyntaxes</c> methods.
        /// </summary>
        /// <param name="result">Result status to return for this proposed presentation context.</param>
        public void SetResult(DicomPresentationContextResult result)
        {
            SetResult(result, _transferSyntaxes[0]);
        }

        /// <summary>
        /// Sets the <c>Result</c> and <c>AcceptedTransferSyntax</c> of this presentation context.
        /// 
        /// The preferred method of accepting presentation contexts is to call one of the <c>AcceptTransferSyntaxes</c> methods.
        /// </summary>
        /// <param name="result">Result status to return for this proposed presentation context.</param>
        /// <param name="acceptedTransferSyntax">Accepted transfer syntax for this proposed presentation context.</param>
        public void SetResult(DicomPresentationContextResult result, DicomTransferSyntax acceptedTransferSyntax)
        {
            _transferSyntaxes.Clear();
            _transferSyntaxes.Add(acceptedTransferSyntax);
            _result = result;
        }

        /// <summary>
        /// Compares a list of transfer syntaxes accepted by the SCP against the list of transfer syntaxes proposed by the SCU. Sets the presentation 
        /// context <c>Result</c> to <c>DicomPresentationContextResult.Accept</c> if an accepted transfer syntax is found. If no accepted transfer
        /// syntax is found, the presentation context <c>Result</c> is set to <c>DicomPresentationContextResult.RejectTransferSyntaxesNotSupported</c>.
        /// </summary>
        /// <param name="acceptedTransferSyntaxes">Transfer syntaxes that the SCP accepts for the proposed abstract syntax.</param>
        /// <returns>Returns <c>true</c> if an accepted transfer syntax was found. Returns <c>false</c> if no accepted transfer syntax was found.</returns>
        public bool AcceptTransferSyntaxes(params DicomTransferSyntax[] acceptedTransferSyntaxes)
        {
            return AcceptTransferSyntaxes(acceptedTransferSyntaxes, false);
        }

        /// <summary>
        /// Compares a list of transfer syntaxes accepted by the SCP against the list of transfer syntaxes proposed by the SCU. Sets the presentation 
        /// context <c>Result</c> to <c>DicomPresentationContextResult.Accept</c> if an accepted transfer syntax is found. If no accepted transfer
        /// syntax is found, the presentation context <c>Result</c> is set to <c>DicomPresentationContextResult.RejectTransferSyntaxesNotSupported</c>.
        /// </summary>
        /// <param name="acceptedTransferSyntaxes">Transfer syntaxes that the SCP accepts for the proposed abstract syntax.</param>
        /// <param name="scpPriority">If set to <c>true</c>, transfer syntaxes will be accepted in the order specified by <paramref name="acceptedTransferSyntaxes"/>. If set to <c>false</c>, transfer syntaxes will be accepted in the order proposed by the SCU.</param>
        /// <returns>Returns <c>true</c> if an accepted transfer syntax was found. Returns <c>false</c> if no accepted transfer syntax was found.</returns>
        public bool AcceptTransferSyntaxes(DicomTransferSyntax[] acceptedTransferSyntaxes, bool scpPriority)
        {
            if (Result == DicomPresentationContextResult.Accept) return true;

            if (scpPriority)
            {
                // let the SCP decide which syntax that it would prefer
                foreach (DicomTransferSyntax ts in acceptedTransferSyntaxes)
                {
                    if (ts != null && HasTransferSyntax(ts))
                    {
                        SetResult(DicomPresentationContextResult.Accept, ts);
                        return true;
                    }
                }
            }
            else
            {
                // accept syntaxes in the order that the SCU proposed them
                foreach (DicomTransferSyntax ts in _transferSyntaxes)
                {
                    if (acceptedTransferSyntaxes.Contains(ts))
                    {
                        SetResult(DicomPresentationContextResult.Accept, ts);
                        return true;
                    }
                }
            }

            SetResult(DicomPresentationContextResult.RejectTransferSyntaxesNotSupported);

            return false;
        }

        /// <summary>
        /// Add transfer syntax.
        /// </summary>
        /// <param name="ts">Transfer syntax to add to presentation context.</param>
        public void AddTransferSyntax(DicomTransferSyntax ts)
        {
            if (ts != null && !_transferSyntaxes.Contains(ts)) _transferSyntaxes.Add(ts);
        }

        /// <summary>
        /// Remove transfer syntax.
        /// </summary>
        /// <param name="ts">Transfer syntax to remove from presentation context.</param>
        public void RemoveTransferSyntax(DicomTransferSyntax ts)
        {
            if (ts != null && _transferSyntaxes.Contains(ts)) _transferSyntaxes.Remove(ts);
        }

        /// <summary>
        /// Clear all supported transfer syntaxes.
        /// </summary>
        public void ClearTransferSyntaxes()
        {
            _transferSyntaxes.Clear();
        }

        /// <summary>
        /// Get read-only list of supported transfer syntaxes.
        /// </summary>
        /// <returns></returns>
        public IList<DicomTransferSyntax> GetTransferSyntaxes()
        {
            return new ReadOnlyCollection<DicomTransferSyntax>(_transferSyntaxes);
        }

        /// <summary>
        /// Checks whether presentation context contains <paramref name="ts">transfer syntax</paramref>.
        /// </summary>
        /// <param name="ts">Transfer syntax to check.</param>
        /// <returns><code>true</code> if <paramref name="ts">transfer syntax</paramref> is supported, <code>false</code> otherwise.</returns>
        public bool HasTransferSyntax(DicomTransferSyntax ts)
        {
            return _transferSyntaxes.Contains(ts);
        }

        /// <summary>
        /// Get user-friendly description of negotiation result.
        /// </summary>
        /// <returns>User-friendly description of negotiation result.</returns>
        public string GetResultDescription()
        {
            switch (_result)
            {
                case DicomPresentationContextResult.Accept:
                    return "Accept";
                case DicomPresentationContextResult.Proposed:
                    return "Proposed";
                case DicomPresentationContextResult.RejectAbstractSyntaxNotSupported:
                    return "Reject - Abstract Syntax Not Supported";
                case DicomPresentationContextResult.RejectNoReason:
                    return "Reject - No Reason";
                case DicomPresentationContextResult.RejectTransferSyntaxesNotSupported:
                    return "Reject - Transfer Syntaxes Not Supported";
                case DicomPresentationContextResult.RejectUser:
                    return "Reject - User";
                default:
                    return "Unknown";
            }
        }

        #endregion
    }
}
