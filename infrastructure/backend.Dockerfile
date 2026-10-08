# Multi-stage Dockerfile for Nitro Rush Spring Boot 3.3 Backend
# Stage 1: Build stage with Maven and OpenJDK 21
FROM maven:3.9-eclipse-temurin-21 AS build
WORKDIR /app

# Cache dependencies
COPY pom.xml .
RUN mvn -q dependency:go-offline

# Copy source code and package application
COPY src ./src
RUN mvn -q package -DskipTests

# Stage 2: Runtime stage with minimal Alpine JRE 21
FROM eclipse-temurin:21-jre-alpine
WORKDIR /app

# Create non-root user for security
RUN addgroup -S nitrogroup && adduser -S nitrouser -G nitrogroup
USER nitrouser

# Copy built artifact from build stage
COPY --from=build /app/target/*.jar app.jar

EXPOSE 8080

# Healthcheck via HTTP Actuator health endpoint
HEALTHCHECK --interval=10s --timeout=3s --retries=3 \
  CMD wget -q --spider http://localhost:8080/actuator/health || exit 1

ENTRYPOINT ["java", "-jar", "app.jar"]
