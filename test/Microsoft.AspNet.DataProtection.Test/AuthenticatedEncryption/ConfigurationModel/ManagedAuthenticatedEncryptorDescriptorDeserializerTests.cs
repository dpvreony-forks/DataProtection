﻿// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Security.Cryptography;
using System.Xml.Linq;
using Xunit;

namespace Microsoft.AspNet.DataProtection.AuthenticatedEncryption.ConfigurationModel
{
    public class ManagedAuthenticatedEncryptorDescriptorDeserializerTests
    {
        [Theory]
        [InlineData(typeof(Aes), typeof(HMACSHA1))]
        [InlineData(typeof(Aes), typeof(HMACSHA256))]
        [InlineData(typeof(Aes), typeof(HMACSHA384))]
        [InlineData(typeof(Aes), typeof(HMACSHA512))]
        public void ImportFromXml_BuiltInTypes_CreatesAppropriateDescriptor(Type encryptionAlgorithmType, Type validationAlgorithmType)
        {
            // Arrange
            var control = new ManagedAuthenticatedEncryptorDescriptor(
                new ManagedAuthenticatedEncryptionOptions()
                {
                    EncryptionAlgorithmType = encryptionAlgorithmType,
                    EncryptionAlgorithmKeySize = 192,
                    ValidationAlgorithmType = validationAlgorithmType
                },
                "k88VrwGLINfVAqzlAp7U4EAjdlmUG17c756McQGdjHU8Ajkfc/A3YOKdqlMcF6dXaIxATED+g2f62wkRRRRRzA==".ToSecret()).CreateEncryptorInstance();

            string xml = String.Format(@"
                <descriptor>
                  <encryption algorithm='{0}' keyLength='192' />
                  <validation algorithm='{1}' />
                  <masterKey enc:requiresEncryption='true' xmlns:enc='http://schemas.asp.net/2015/03/dataProtection'>
                    <value>k88VrwGLINfVAqzlAp7U4EAjdlmUG17c756McQGdjHU8Ajkfc/A3YOKdqlMcF6dXaIxATED+g2f62wkRRRRRzA==</value>
                  </masterKey>
                </descriptor>",
                 encryptionAlgorithmType.Name, validationAlgorithmType.Name);
            var test = new ManagedAuthenticatedEncryptorDescriptorDeserializer().ImportFromXml(XElement.Parse(xml)).CreateEncryptorInstance();

            // Act & assert
            byte[] plaintext = new byte[] { 1, 2, 3, 4, 5 };
            byte[] aad = new byte[] { 2, 4, 6, 8, 0 };
            byte[] ciphertext = control.Encrypt(new ArraySegment<byte>(plaintext), new ArraySegment<byte>(aad));
            byte[] roundTripPlaintext = test.Decrypt(new ArraySegment<byte>(ciphertext), new ArraySegment<byte>(aad));
            Assert.Equal(plaintext, roundTripPlaintext);
        }

        [Fact]
        public void ImportFromXml_CustomType_CreatesAppropriateDescriptor()
        {
            // Arrange
            var control = new ManagedAuthenticatedEncryptorDescriptor(
                new ManagedAuthenticatedEncryptionOptions()
                {
                    EncryptionAlgorithmType = typeof(AesCryptoServiceProvider),
                    EncryptionAlgorithmKeySize = 192,
                    ValidationAlgorithmType = typeof(HMACSHA384)
                },
                "k88VrwGLINfVAqzlAp7U4EAjdlmUG17c756McQGdjHU8Ajkfc/A3YOKdqlMcF6dXaIxATED+g2f62wkRRRRRzA==".ToSecret()).CreateEncryptorInstance();

            string xml = String.Format(@"
                <descriptor>
                  <encryption algorithm='{0}' keyLength='192' />
                  <validation algorithm='{1}' />
                  <masterKey enc:requiresEncryption='true' xmlns:enc='http://schemas.asp.net/2015/03/dataProtection'>
                    <value>k88VrwGLINfVAqzlAp7U4EAjdlmUG17c756McQGdjHU8Ajkfc/A3YOKdqlMcF6dXaIxATED+g2f62wkRRRRRzA==</value>
                  </masterKey>
                </descriptor>",
                 typeof(AesCryptoServiceProvider).AssemblyQualifiedName, typeof(HMACSHA384).AssemblyQualifiedName);
            var test = new ManagedAuthenticatedEncryptorDescriptorDeserializer().ImportFromXml(XElement.Parse(xml)).CreateEncryptorInstance();

            // Act & assert
            byte[] plaintext = new byte[] { 1, 2, 3, 4, 5 };
            byte[] aad = new byte[] { 2, 4, 6, 8, 0 };
            byte[] ciphertext = control.Encrypt(new ArraySegment<byte>(plaintext), new ArraySegment<byte>(aad));
            byte[] roundTripPlaintext = test.Decrypt(new ArraySegment<byte>(ciphertext), new ArraySegment<byte>(aad));
            Assert.Equal(plaintext, roundTripPlaintext);
        }
    }
}
