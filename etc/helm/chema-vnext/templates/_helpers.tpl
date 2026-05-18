{{- define "chema-vnext.name" -}}
{{- default .Chart.Name .Values.nameOverride | trunc 63 | trimSuffix "-" -}}
{{- end -}}

{{- define "chema-vnext.fullname" -}}
{{- if .Values.fullnameOverride -}}
{{- .Values.fullnameOverride | trunc 63 | trimSuffix "-" -}}
{{- else -}}
{{- printf "%s-%s" .Release.Name (include "chema-vnext.name" .) | trunc 63 | trimSuffix "-" -}}
{{- end -}}
{{- end -}}

{{- define "chema-vnext.componentName" -}}
{{- $root := index . "root" -}}
{{- $name := index . "name" -}}
{{- printf "%s-%s" $root.Release.Name $name | trunc 63 | trimSuffix "-" -}}
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

{{- define "chema-vnext.secretName" -}}
{{- if .Values.secret.create -}}
{{- .Values.secret.name -}}
{{- else -}}
{{- default .Values.secret.name .Values.secret.existingSecret -}}
{{- end -}}
{{- end -}}

{{- define "chema-vnext.componentUrl" -}}
{{- $root := index . "root" -}}
{{- $name := index . "name" -}}
{{- $port := index . "port" -}}
{{- printf "http://%s:%v" (include "chema-vnext.componentName" (dict "root" $root "name" $name)) $port -}}
{{- end -}}

{{- define "chema-vnext.httpapiUrl" -}}
{{- include "chema-vnext.componentUrl" (dict "root" . "name" .Values.services.httpapiHost.name "port" .Values.services.httpapiHost.port) -}}
{{- end -}}

{{- define "chema-vnext.gatewayUrl" -}}
{{- include "chema-vnext.componentUrl" (dict "root" . "name" .Values.services.gateway.name "port" .Values.services.gateway.port) -}}
{{- end -}}

{{- define "chema-vnext.blazorUrl" -}}
{{- include "chema-vnext.componentUrl" (dict "root" . "name" .Values.services.blazor.name "port" .Values.services.blazor.port) -}}
{{- end -}}
