using System.Text;
using Yarp.ReverseProxy.Forwarder;

namespace WolfLeash.Components.Classes;

public class ReverseProxyHtmlInjectionHandler : DelegatingHandler
{
    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        var response = await base.SendAsync(request, cancellationToken);

        // Check if the response is HTML
        var contentType = response.Content.Headers.ContentType?.MediaType;
        if (response.IsSuccessStatusCode && contentType != null && contentType.Contains("text/html", StringComparison.OrdinalIgnoreCase))
        {
            var originalContent = await response.Content.ReadAsStringAsync(cancellationToken);

            var injectionScript = """
                                      <script>
                                          const originalRTCPeerConnection = window.RTCPeerConnection;
                                          window.RTCPeerConnection = function(...args) {
                                              const pc = new originalRTCPeerConnection(...args);
                                              
                                              pc.addEventListener('connectionstatechange', () => {
                                                  if (pc.connectionState === 'disconnected' || 
                                                      pc.connectionState === 'failed' || 
                                                      pc.connectionState === 'closed') {
                                                      
                                                      // Give it a brief moment to settle, then redirect back to dashboard
                                                      setTimeout(() => {
                                                          window.location.href = '/moonlight/';
                                                      }, 50);
                                                  }
                                              });
                                              
                                              return pc;
                                          };

                                          const originalOpen = window.open;
                                          window.open = function(url, target, features) {
                                              window.location.href = url;
                                              return window;
                                          };

                                          document.addEventListener('click', function(e) {
                                              const anchor = e.target.closest('a');
                                              if (anchor && anchor.target === '_blank') {
                                                  anchor.target = '_self';
                                              }
                                          }, true);
                                      </script>
                                      </head>
                                  """;
            
            var modifiedContent = originalContent.Replace("</head>", injectionScript, StringComparison.OrdinalIgnoreCase);
            
            var buffer = Encoding.UTF8.GetBytes(modifiedContent);
            response.Content = new ByteArrayContent(buffer);
            response.Content.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("text/html");
            response.Content.Headers.ContentLength = buffer.Length;
        }

        return response;
    }
}

public class ReverseProxyHtmlInjectionClientFactory : ForwarderHttpClientFactory
{
    protected override HttpMessageHandler WrapHandler(ForwarderHttpClientContext context, HttpMessageHandler handler)
    {
        var injectionHandler = new ReverseProxyHtmlInjectionHandler
        {
            InnerHandler = handler
        };

        return base.WrapHandler(context, injectionHandler);
    }
}