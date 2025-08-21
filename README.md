```bash
odata-cli --roll-forward LatestMajor generate --metadata-uri "https://localhost:7007/$metadata" --namespace-prefix "OData.Demo.OData.Client" --service-name "ODataApi" --outputdir "Connected Services/ODataApi" --omit-versioning-info --enable-tracking --ignore-unexpected-elements
```

```bash
odata-cli --roll-forward LatestMajor generate --config-file "Connected Services/ODataApi/ConnectedService.json" --outputdir "Connected Services/ODataApi"
```