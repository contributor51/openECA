﻿//******************************************************************************************************
//  ProjectGenerator.cs - Gbtc
//
//  Copyright © 2016, Grid Protection Alliance.  All Rights Reserved.
//
//  Licensed to the Grid Protection Alliance (GPA) under one or more contributor license agreements. See
//  the NOTICE file distributed with this work for additional information regarding copyright ownership.
//  The GPA licenses this file to you under the MIT License (MIT), the "License"; you may
//  not use this file except in compliance with the License. You may obtain a copy of the License at:
//
//      http://opensource.org/licenses/MIT
//
//  Unless agreed to in writing, the subject software distributed under the License is distributed on an
//  "AS-IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. Refer to the
//  License for the specific language governing permissions and limitations.
//
//  Code Modification History:
//  ----------------------------------------------------------------------------------------------------
//  07/08/2016 - J. Ritchie Carroll
//       Generated original version of source code.
//
//******************************************************************************************************

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ECAClientUtilities.Model;

namespace ECAClientUtilities.Template.Matlab
{
    public class ProjectGenerator : DotNetProjectGeneratorBase
    {
        #region [ Members ]

        private readonly Dictionary<string, string> m_primitiveDefaultValues;
        private readonly Dictionary<string, Tuple<string, bool>> m_primitiveConversionFunctions;

        #endregion

        #region [ Constructors ]

        public ProjectGenerator(string projectName, MappingCompiler compiler) : base(projectName, compiler, "m", "Matlab")
        {
            m_primitiveDefaultValues = new Dictionary<string, string>()
            {
                { "Integer.Byte", "0" },
                { "Integer.Int16", "0" },
                { "Integer.Int32", "0" },
                { "Integer.Int64", "0" },
                { "Integer.UInt16", "0" },
                { "Integer.UInt32", "0" },
                { "Integer.UInt64", "0" },
                { "FloatingPoint.Decimal", "0.0" },
                { "FloatingPoint.Double", "0.0" },
                { "FloatingPoint.Single", "0.0" },
                { "DateTime.Date", "System.DateTime.MinValue" },
                { "DateTime.DateTime", "System.DateTime.MinValue" },
                { "DateTime.Time", "System.DateTime.MinValue" },
                { "DateTime.TimeSpan", "System.TimeSpan.MinValue" },
                { "Text.Char", "''" },
                { "Text.String", "''" },
                { "Other.Boolean", "0" },
                { "Other.Guid", "System.Guid.Empty" }
            };

            m_primitiveConversionFunctions = new Dictionary<string, Tuple<string, bool>>
            {
                { "Integer.Byte", new Tuple<string, bool>("uint8", false) },
                { "Integer.Int16", new Tuple<string, bool>("int16", false) },
                { "Integer.Int32", new Tuple<string, bool>("int32", false) },
                { "Integer.Int64", new Tuple<string, bool>("int64", false) },
                { "Integer.UInt16", new Tuple<string, bool>("uint16", false) },
                { "Integer.UInt32", new Tuple<string, bool>("uint32", false) },
                { "Integer.UInt64", new Tuple<string, bool>("uint64", false) },
                { "FloatingPoint.Decimal", new Tuple<string, bool>("double", false) },
                { "FloatingPoint.Double", new Tuple<string, bool>("double", false) },
                { "FloatingPoint.Single", new Tuple<string, bool>("single", false) },
                { "DateTime.Date", new Tuple<string, bool>("DateTime.Parse", true) },
                { "DateTime.DateTime", new Tuple<string, bool>("DateTime.Parse", true) },
                { "DateTime.Time", new Tuple<string, bool>("DateTime.Parse", true) },
                { "DateTime.TimeSpan", new Tuple<string, bool>("TimeSpan.Parse", true) },
                { "Text.Char", new Tuple<string, bool>("char", false) },
                { "Text.String", new Tuple<string, bool>("char", false) },
                { "Other.Boolean", new Tuple<string, bool>("logical", false) },
                { "Other.Guid", new Tuple<string, bool>("Guid.Parse", true) }
            };
        }

        #endregion

        #region [ Methods ]

        protected override string ConstructModel(UserDefinedType type)
        {
            string fieldList = string.Join(", ", type.Fields.Select(field => $"'{field.Identifier}', {GetDefaultValue(field.Type)}"));

            // Generate the contents of the class file
            return GetTextFromResource("ECAClientUtilities.Template.Matlab.UDTTemplate.txt")
                .Replace("{ProjectName}", ProjectName)
                .Replace("{Category}", type.Category)
                .Replace("{Identifier}", type.Identifier)
                .Replace("{Fields}", fieldList.Trim());
        }

        protected override string ConstructMapping(UserDefinedType type)
        {
            StringBuilder mappingCode = new StringBuilder();

            foreach (UDTField field in type.Fields)
            {
                // Get the field type and its
                // underlying type if it is an array
                DataType fieldType = field.Type;
                DataType underlyingType = (field.Type as ArrayType)?.UnderlyingType;

                // For user-defined types, call the method to generate an object of their corresponding data type
                // For primitive types, call the method to get the values of the mapped measurements
                // ReSharper disable once PossibleNullReferenceException
                if (fieldType.IsArray && underlyingType.IsUserDefined)
                {
                    mappingCode.AppendLine($"            typeMappings = NET.invokeGenericMethod('System.Linq.Enumerable', 'ToArray', {{'ECAClientUtilities.Model.TypeMapping'}}, this.m_helper.MappingCompiler.EnumerateTypeMappings(fieldLookup.Item('{field.Identifier}').Expression));");
                    mappingCode.AppendLine();
                    mappingCode.AppendLine("            for i = 1:typeMappings.Length()");
                    mappingCode.AppendLine($"                udt.Phasors(i) = this.Create{underlyingType.Category}{underlyingType.Identifier}(typeMappings(i));");
                    mappingCode.AppendLine("            end");
                }
                else if (fieldType.IsUserDefined)
                {
                    mappingCode.AppendLine($"            udt.{field.Identifier} = this.Create{field.Type.Category}{field.Type.Identifier}(this.m_helper.MappingCompiler.GetTypeMapping(fieldLookup.Item('{field.Identifier}').Expression));");
                }
                else if (fieldType.IsArray)
                {
                    bool forceToString;
                    string conversionFunction = GetConversionFunction(underlyingType, out forceToString);

                    mappingCode.AppendLine("            measurements = this.m_helper.SignalLookup.GetMeasurements(this.m_helper.Keys.Item(this.m_index));");
                    mappingCode.AppendLine("            this.m_index = this.m_index + 1;");
                    mappingCode.AppendLine();
                    mappingCode.AppendLine("            for i = 1:measurements.Length()");
                    mappingCode.AppendLine($"                udt.{field.Identifier} (i) = {conversionFunction}(measurements(i).Value{(forceToString ? ".ToString()" : "")});");
                    mappingCode.AppendLine("            end");
                }
                else
                {
                    bool forceToString;
                    string conversionFunction = GetConversionFunction(field.Type, out forceToString);

                    mappingCode.AppendLine($"            udt.{field.Identifier} = {conversionFunction}(this.m_helper.SignalLookup.GetMeasurement(this.m_helper.Keys.Item(this.m_index).Get(0)).Value{(forceToString ? ".ToString()" : "")});");
                    mappingCode.AppendLine("            this.m_index = this.m_index + 1;");
                }

                mappingCode.AppendLine();
            }

            return mappingCode.ToString();
        }

        protected override string ConstructUsing(UserDefinedType type)
        {
            return $"addpath('Model/{type.Category}/');";
        }

        protected override Dictionary<string, string> GetPrimitiveTypeMap()
        {
            return new Dictionary<string, string>
            {
                { "Integer.Byte", "uint8" },
                { "Integer.Int16", "int16" },
                { "Integer.Int32", "int32" },
                { "Integer.Int64", "int64" },
                { "Integer.UInt16", "uint16" },
                { "Integer.UInt32", "uint32" },
                { "Integer.UInt64", "uint64" },
                { "FloatingPoint.Decimal", "double" },
                { "FloatingPoint.Double", "double" },
                { "FloatingPoint.Single", "single" },
                { "DateTime.Date", "System.DateTime" },
                { "DateTime.DateTime", "System.DateTime" },
                { "DateTime.Time", "System.DateTime" },
                { "DateTime.TimeSpan", "System.TimeSpan" },
                { "Text.Char", "char" },
                { "Text.String", "char" },
                { "Other.Boolean", "logical" },
                { "Other.Guid", "System.Guid" }
            };
        }

        protected override void UpdateProjectFile(string projectPath, List<UserDefinedType> orderedInputTypes)
        {
            // MATLAB template doesn't use a project file...
        }

        private string GetDefaultValue(DataType type)
        {
            DataType underlyingType = (type as ArrayType)?.UnderlyingType ?? type;
            string defaultValue;

            // Namespace: ProjectName.Model.Category
            // TypeName: Identifier
            if (!m_primitiveDefaultValues.TryGetValue($"{underlyingType.Category}.{underlyingType.Identifier}", out defaultValue))
                defaultValue = $"{underlyingType.Identifier}()";

            return defaultValue;
        }

        private string GetConversionFunction(DataType type, out bool forceToString)
        {
            DataType underlyingType = (type as ArrayType)?.UnderlyingType ?? type;
            Tuple<string, bool> conversion;

            // Namespace: ProjectName.Model.Category
            // TypeName: Identifier
            if (!m_primitiveConversionFunctions.TryGetValue($"{underlyingType.Category}.{underlyingType.Identifier}", out conversion))
                throw new InvalidOperationException($"Unexpected primitive type encountered: \"{underlyingType.Category}.{underlyingType.Identifier}\"");

            forceToString = conversion.Item2;
            return conversion.Item1;
        }

        #endregion
    }
}
