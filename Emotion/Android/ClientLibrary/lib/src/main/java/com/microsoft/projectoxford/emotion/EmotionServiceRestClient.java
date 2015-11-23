//
// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license.
//
// Project Oxford: http://ProjectOxford.ai
//
// ProjectOxford SDK Github:
// https://github.com/Microsoft/ProjectOxfordSDK-Windows
//
// Copyright (c) Microsoft Corporation
// All rights reserved.
//
// MIT License:
// Permission is hereby granted, free of charge, to any person obtaining
// a copy of this software and associated documentation files (the
// "Software"), to deal in the Software without restriction, including
// without limitation the rights to use, copy, modify, merge, publish,
// distribute, sublicense, and/or sell copies of the Software, and to
// permit persons to whom the Software is furnished to do so, subject to
// the following conditions:
//
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED ""AS IS"", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
// LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
// OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
// WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
//
package com.microsoft.projectoxford.emotion;

import com.google.gson.Gson;
import com.microsoft.projectoxford.emotion.contract.RecognizeResult;
import com.microsoft.projectoxford.emotion.rest.EmotionServiceException;
import com.microsoft.projectoxford.emotion.rest.WebServiceRequest;

import org.apache.commons.io.IOUtils;

import java.io.IOException;
import java.io.InputStream;
import java.util.Arrays;
import java.util.HashMap;
import java.util.List;
import java.util.Map;

public class EmotionServiceRestClient implements EmotionServiceClient {
    private static final String serviceHost = "https://api.projectoxford.ai/emotion/v1.0";
    private WebServiceRequest restCall = null;
    private Gson gson = new Gson();

    public EmotionServiceRestClient(String subscriptKey) {
        this.restCall = new WebServiceRequest(subscriptKey);
    }

    @Override
    public List<RecognizeResult> recognizeImage(String url) throws EmotionServiceException {
        Map<String, Object> params = new HashMap<>();
        String path = serviceHost + "/recognize";
        String uri = WebServiceRequest.getUrl(path, params);

        params.clear();
        params.put("url", url);

        String json = (String) this.restCall.request(uri, "POST", params, null, false);
        RecognizeResult[] recognizeResult = this.gson.fromJson(json, RecognizeResult[].class);

        return Arrays.asList(recognizeResult);
    }

    @Override
    public List<RecognizeResult> recognizeImage(InputStream stream) throws EmotionServiceException, IOException {
        Map<String, Object> params = new HashMap<>();
        String path = serviceHost + "/recognize";
        String uri = WebServiceRequest.getUrl(path, params);

        params.clear();
        byte[] data = IOUtils.toByteArray(stream);
        params.put("data", data);

        String json = (String) this.restCall.request(uri, "POST", params, "application/octet-stream", false);
        RecognizeResult[] recognizeResult = this.gson.fromJson(json, RecognizeResult[].class);

        return Arrays.asList(recognizeResult);
    }
}