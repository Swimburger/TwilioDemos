Uses ngrok or a similar service and 
configure the message webhook with _[TUNNEL_URL]/message_ 
and the voice webhook with _[TUNNEL_USER]/voice_.

```bash
ngrok http http://localhost:5189 --subdomain swimburger-demos
```

(I'm using this subdomain (paid ngrok) for my convenience, but not necessary.)