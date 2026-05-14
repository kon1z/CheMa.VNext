{{- define "chema-vnext.name" -}}
{{- default .Chart.Name .Values.nameOverride | trunc 63 | trimSuffix "-" -}}
{{- end -}}

{{- define "chema-vnext.fullname" -}}
{{- if .Values.fullnameOverride -}}
{{- .Values.fullnameOverride | trunc 63 | trimSuffix "-" -}}
{{- else -}}
{{- include "chema-vnext.name" . -}}
{{- end -}}
{{- end -}}

{{- define "chema-vnext.labels" -}}
helm.sh/chart: {{ .Chart.Name }}-{{ .Chart.Version | replace "+" "_" }}
app.kubernetes.io/part-of: chema-vnext
app.kubernetes.io/managed-by: {{ .Release.Service }}
app.kubernetes.io/instance: {{ .Release.Name }}
app.kubernetes.io/version: {{ .Chart.AppVersion | quote }}
{{- end -}}

{{- define "chema-vnext.image" -}}
{{- $root := index . "root" -}}
{{- $imageName := index . "imageName" -}}
{{- printf "%s/%s/%s:%s" ($root.Values.image.registry | trimSuffix "/") ($root.Values.image.repositoryPrefix | trimAll "/") $imageName $root.Values.image.tag -}}
{{- end -}}

{{- define "chema-vnext.imagePullSecrets" -}}
{{- with .Values.imagePullSecrets }}
imagePullSecrets:
{{- toYaml . | nindent 2 }}
{{- end }}
{{- end -}}

{{- define "chema-vnext.serviceAccountName" -}}
{{- if .Values.serviceAccount.create -}}
{{- default (include "chema-vnext.fullname" .) .Values.serviceAccount.name -}}
{{- else -}}
{{- default "default" .Values.serviceAccount.name -}}
{{- end -}}
{{- end -}}
