{{- define "helpers.list-api-deployment-env-variables" }}
{{- range $key, $val := .Values.api.env.nonsecret }}
- name: {{ $key }}
  value: {{ $val | quote }}
{{- end }}
{{- range $key := .Values.api.env.secret }}
- name: {{ $key }}
  valueFrom:
    secretKeyRef:
      name: 'zeno-torrent-secret'
      key: {{ $key }}
{{- end}}
{{- end }}

{{- define "helpers.list-bff-deployment-env-variables" }}
{{- range $key, $val := .Values.bff.env.nonsecret }}
- name: {{ $key }}
  value: {{ $val | quote }}
{{- end }}
{{- range $key := .Values.bff.env.secret }}
- name: {{ $key }}
  valueFrom:
    secretKeyRef:
      name: 'zeno-torrent-secret'
      key: {{ $key }}
{{- end}}
{{- end }}

{{- define "helpers.list-web-deployment-env-variables" }}
{{- range $key, $val := .Values.web.env.nonsecret }}
- name: {{ $key }}
  value: {{ $val | quote }}
{{- end }}
{{- range $key := .Values.web.env.secret }}
- name: {{ $key }}
  valueFrom:
    secretKeyRef:
      name: 'zeno-torrent-secret'
      key: {{ $key }}
{{- end}}
{{- end }}