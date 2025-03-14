namespace Project.Configurations
{
    public static class PipelineConfiguration
    {
        public static WebApplication ConfigurePipeline(this WebApplication app)
        {
            app.UseHttpsRedirection();
            app.UseStaticFiles();
            app.UseRouting();
            app.UseAuthorization();
            return app;
        }
    }
}
