﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace DevelopmentInProgress.TradeView.Core.TradeStrategy
{
    public static class StrategyRunnerClient
    {
        public static async Task<HttpResponseMessage> PostAsync(HttpClient httpClient, Uri uri, string jsonSerializedStrategy, IEnumerable<string> libraries, CancellationToken cancellationToken)
        {
            if (httpClient == null)
            {
                throw new ArgumentNullException(nameof(httpClient));
            }

            if (libraries == null)
            {
                throw new ArgumentNullException(nameof(libraries));
            }

            var stringContent = new StringContent(jsonSerializedStrategy, Encoding.UTF8, "application/json");

            var byteArrayContents = new List<ByteArrayContent>();

            try
            {
                using var client = new HttpClient();
                using var multipartFormDataContent = new MultipartFormDataContent
                {
                    { stringContent, "strategy" }
                };

                foreach (var file in libraries)
                {
                    var fileInfo = new FileInfo(file);
                    using var fileStream = File.OpenRead(file);
                    using var br = new BinaryReader(fileStream);
                    var byteArrayContent = new ByteArrayContent(br.ReadBytes((int)fileStream.Length));
                    multipartFormDataContent.Add(byteArrayContent, fileInfo.Name, fileInfo.FullName);
                    byteArrayContents.Add(byteArrayContent);
                }

                return await httpClient.PostAsync(uri, multipartFormDataContent, cancellationToken).ConfigureAwait(false);
            }
            finally
            {
                foreach (var byteArrayContent in byteArrayContents)
                {
                    byteArrayContent.Dispose();
                }

                stringContent.Dispose();
            }
        }

        public static async Task<HttpResponseMessage> PostAsync(HttpClient httpClient, Uri uri, string jsonSerializedStrategyParameters, CancellationToken cancellationToken)
        {
            if (httpClient == null)
            {
                throw new ArgumentNullException(nameof(httpClient));
            }

            var stringContent = new StringContent(jsonSerializedStrategyParameters, Encoding.UTF8, "application/json");

            try
            {
                using var multipartFormDataContent = new MultipartFormDataContent
                {
                    { stringContent, "strategyparameters" }
                };

                return await httpClient.PostAsync(uri, multipartFormDataContent, cancellationToken).ConfigureAwait(false);
            }
            finally
            {
                stringContent.Dispose();
            }
        }
    }
}