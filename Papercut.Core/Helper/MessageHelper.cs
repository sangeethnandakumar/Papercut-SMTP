﻿/*  
 * Papercut
 *
 *  Copyright © 2008 - 2012 Ken Robertson
 *  Copyright © 2013 - 2014 Jaben Cargman
 *  
 *  Licensed under the Apache License, Version 2.0 (the "License");
 *  you may not use this file except in compliance with the License.
 *  You may obtain a copy of the License at
 *  
 *  http://www.apache.org/licenses/LICENSE-2.0
 *  
 *  Unless required by applicable law or agreed to in writing, software
 *  distributed under the License is distributed on an "AS IS" BASIS,
 *  WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 *  See the License for the specific language governing permissions and
 *  limitations under the License.
 *  
 */

namespace Papercut.Core.Helper
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    using Microsoft.Win32;

    using MimeKit;

    using Papercut.Core.Annotations;

    public static class MessageHelper
    {
        public static bool IsContentHtml([NotNull] this TextPart textPart)
        {
            return textPart.ContentType.Matches("text", "html");
        }

        public static string GetExtension([NotNull] this ContentType contentType)
        {
            if (contentType == null) throw new ArgumentNullException("contentType");

            return
                Registry.ClassesRoot.OpenSubKey(
                    string.Format(
                        @"MIME\Database\Content Type\{0}/{1}",
                        contentType.MediaType,
                        contentType.MediaSubtype),
                    false)
                    .ToEnumerable()
                    .Select(k => k.GetValue("Extension", null))
                    .Where(v => v != null)
                    .Select(v => v.ToString())
                    .FirstOrDefault();
        }

        public static IEnumerable<MimePart> GetImages(
            [NotNull] this IEnumerable<MimePart> prefilteredMimeParts)
        {
            if (prefilteredMimeParts == null) throw new ArgumentNullException("prefilteredMimeParts");

            return prefilteredMimeParts.Where(e => e.ContentType.Matches("image", "*"));
        }

        public static IEnumerable<MimePart> GetAttachments(
            [NotNull] this IEnumerable<MimePart> prefilteredMimeParts)
        {
            if (prefilteredMimeParts == null) throw new ArgumentNullException("prefilteredMimeParts");

            return prefilteredMimeParts.Where(p => p.IsAttachment);
        }

        public static TextPart GetMainBodyTextPart(
            [NotNull] this IEnumerable<MimePart> prefilteredMimeParts)
        {
            List<TextPart> mimeParts =
                prefilteredMimeParts
                    .OfType<TextPart>()
                    .Where(s => !s.IsAttachment)
                    .ToList();

            // return html if available first
            TextPart html = mimeParts.FirstOrDefault(s => s.IsContentHtml());

            if (!html.IsDefault()) return html;

            // anything else available
            return mimeParts.FirstOrDefault();
        }
    }
}