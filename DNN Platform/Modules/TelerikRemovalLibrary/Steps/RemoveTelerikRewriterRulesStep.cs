﻿// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace Dnn.Modules.TelerikRemovalLibrary
{
    using System;
    using System.Linq;
    using System.Xml;

    using DotNetNuke.Abstractions.Application;
    using DotNetNuke.Instrumentation;

    /// <inheritdoc/>
    internal class RemoveTelerikRewriterRulesStep : XmlStepBase, IRemoveTelerikRewriterRulesStep
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="RemoveTelerikRewriterRulesStep"/> class.
        /// </summary>
        /// <param name="loggerSource">An instance of <see cref="ILoggerSource"/>.</param>
        /// <param name="applicationStatusInfo">An instance of <see cref="IApplicationStatusInfo"/>.</param>
        public RemoveTelerikRewriterRulesStep(ILoggerSource loggerSource, IApplicationStatusInfo applicationStatusInfo)
            : base(loggerSource, applicationStatusInfo)
        {
        }

        /// <inheritdoc/>
        public override string Name => "Remove Telerik URL rewrite rules";

        /// <inheritdoc/>
        [Required]
        public override string RelativeFilePath => "Config/SiteUrls.config";

        /// <inheritdoc/>
        protected override void ProcessXml(XmlDocument doc)
        {
            const string RulesPath = "/RewriterConfig/Rules";

            this.Success = true;

            var rules = doc.SelectSingleNode(RulesPath);
            if (rules is null)
            {
                this.Notes = $"{RulesPath} section not found.";
                return;
            }

            var matchCount = 0;

            rules.SelectNodes("RewriterRule")
                .Cast<XmlElement>()
                .Select(e => new { RewriterRule = e, LookFor = (XmlElement)e.SelectSingleNode("LookFor") })
                .Where(x => x.LookFor != null && x.LookFor.InnerText != null)
                .Where(x => x.LookFor.InnerText.IndexOf("telerik", StringComparison.OrdinalIgnoreCase) >= 0)
                .Select(x => x.RewriterRule)
                .ToList()
                .ForEach(rewriterRule =>
                {
                    rules.RemoveChild(rewriterRule);
                    matchCount++;
                });

            this.Notes = matchCount > 0 ? $"{matchCount} matches found." : "No matches found.";
        }
    }
}
