# ─── Stage 1: Build ─────────────────────────────────────────────────────────
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Copy project files trước để tận dụng Docker layer cache khi restore
COPY TapHoa.slnx ./
COPY src/Core/TapHoa.Domain/TapHoa.Domain.csproj                              src/Core/TapHoa.Domain/
COPY src/Core/TapHoa.Application/TapHoa.Application.csproj                    src/Core/TapHoa.Application/
COPY src/Infrastructure/TapHoa.Infrastructure/TapHoa.Infrastructure.csproj    src/Infrastructure/TapHoa.Infrastructure/
COPY src/Infrastructure/TapHoa.Persistence/TapHoa.Persistence.csproj          src/Infrastructure/TapHoa.Persistence/
COPY src/SharedKernel/TapHoa.Utilities/TapHoa.Utilities.csproj                src/SharedKernel/TapHoa.Utilities/
COPY src/Presentation/TapHoa.Api/TapHoa.Api.csproj                            src/Presentation/TapHoa.Api/

RUN dotnet restore src/Presentation/TapHoa.Api/TapHoa.Api.csproj

# Copy toàn bộ source (tests đã bị loại bởi .dockerignore)
COPY src/ src/

RUN dotnet publish src/Presentation/TapHoa.Api/TapHoa.Api.csproj \
        --configuration Release \
        --no-restore \
        --output /app/publish

# ─── Stage 2: Runtime ────────────────────────────────────────────────────────
# aspnet image nhỏ hơn sdk ~3x, phù hợp RAM 512MB của Render Free
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app

# Render yêu cầu ứng dụng lắng nghe trên cổng 8080
ENV ASPNETCORE_HTTP_PORTS=8080
EXPOSE 8080

COPY --from=build /app/publish .

# nlog.config không được dotnet publish copy mặc định — copy thủ công
COPY --from=build /src/src/Presentation/TapHoa.Api/nlog.config ./nlog.config

# Thư mục lưu file upload (Program.cs gọi Directory.CreateDirectory khi start,
# nhưng tạo sẵn đảm bảo quyền ghi đúng trong container)
RUN mkdir -p storage/uploads

# Placeholder config — giá trị thực được inject qua Render Environment Variables:
#   ConnectionStrings__DefaultConnection
#   Jwt__Key  |  Jwt__Issuer  |  Jwt__Audience
#   RabbitMQ__Host  |  RabbitMQ__Username  |  RabbitMQ__Password
RUN mkdir -p config && printf '{\n\
  "ConnectionStrings": { "DefaultConnection": "" },\n\
  "Jwt": { "Key": "", "Issuer": "TapHoaAPI", "Audience": "TapHoaClient" },\n\
  "RabbitMQ": { "Host": "", "Username": "", "Password": "" }\n\
}\n' > config/appsettings.json

ENTRYPOINT ["dotnet", "TapHoa.Api.dll"]
