# z21_exporter
Prometheus exporter for the digital command center z21/Z21 from Roco/Fleischmann.

# Install

Get the linux arm image from [dockerhub](https://hub.docker.com/repository/docker/jakobeichberger/z21exporter).


Use the following command to run the container on an arm based linux machine:

```
docker run -d -p 9101:9101 --name z21_exporter --restart=always jakobeichberger/z21exporter:latest
```

Or use it in a docker-compose file:
```
  z21_exporter:
    image: jakobeichberger/z21exporter:latest
    container_name: z21_exporter
    expose:
      - 9101
    restart: always
```

# Sample prometheus config
```
  - job_name: 'z21_exporter'
    static_configs:
      - targets: ['z21_exporter:9101']
```
# Grafana dashboard
Get the grafana dashboard from [here](https://github.com/Jakob-Eichberger/z21_exporter/blob/Grafana/Grafana.JSON). To install select `Dashboards` -> `Install` and paste the JSON into the `Import via panel json` textbox.
![image](https://user-images.githubusercontent.com/53713395/191594567-b7555249-4178-4d0c-afa3-71770a37f88c.png)

