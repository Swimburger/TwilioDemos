
Tunnel the opt-in service using ngrok or a similar service and configure the message webhook with _[TUNNEL_URL]/message_ and SendGrid inbound parse webhook with _[TUNNEL_URL]/email_.

```bash
ngrok http http://localhost:5156 --subdomain swimburger-optin
```

(I'm using this subdomain (paid ngrok) for my convenience, but not necessary.)