export type HealthStatus = "Healthy" | "Degraded" | "Unhealthy";

export type HealthCheck = {
  name: string;
  status: HealthStatus;
  durationMs: number;
};

export type HealthDetails = {
  status: HealthStatus;
  uptimeSeconds: number;
  checkedAt: string;
  checks: HealthCheck[];
};
