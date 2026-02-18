using JobBank.Management;

namespace WORKSCommons.Tests;

[TestClass]
public class RankingEngineTest
{
    private JobDescriptionParser parser;
    private RankingEngine rankingEngine;

    public RankingEngineTest()
    {        
        parser = new JobDescriptionParser();
        rankingEngine = new RankingEngine();
    }

    [TestMethod]
    public void CalcularRankingTest()
    {
        // Arrange
        var jobDescriptionReal = "What You’ll Do\r\nDesign, build, and maintain scalable backend services and APIs using .NET and C#\r\nLead the development of LLM-powered workflows and Voice AI agents used in production healthcare systems\r\nUse AI-assisted tools daily to support coding, code reviews, documentation, system design, and problem-solving\r\nParticipate in architecture discussions and design reviews, ensuring systems meet quality, scalability, security, and compliance standards\r\nOwn services end-to-end, including feature development, bug fixes, performance improvements, and technology upgrades\r\nIntegrate internal services, third-party APIs, and AI platforms via secure, reliable RESTful APIs\r\nBuild and improve CI/CD pipelines and contribute to infrastructure design using infrastructure-as-code\r\nSupport production systems, help resolve escalations, and proactively identify operational risks\r\nCollaborate closely with Product Management, Operations, and other engineering teams in an agile environment\r\nMentor and support other engineers, setting best practices for system design, code quality, and responsible AI-assisted development\r\nContribute to sprint planning, estimation, execution, retrospectives, and occasional facilitation of agile ceremonies\r\n\r\nWhat You Bring\r\nBachelor’s degree in Computer Science or a related field, or equivalent practical experience\r\n6+ years of experience building, scaling, and supporting distributed systems, with strong hands-on experience in C# / .NET (or Java)\r\n2+ years of experience designing, building, or integrating LLM-based systems and/or Voice AI agents in production\r\nProven experience using AI tools as part of everyday engineering work (coding, documentation, design, and technical reasoning)\r\nExperience building client-side applications using React, Angular, or similar frameworks\r\nExperience implementing and maintaining CI/CD pipelines\r\nExperience designing and operating systems on AWS, including infrastructure-as-code (e.g., Terraform)\r\nStrong understanding of software quality, security, reliability, and operational best practices\r\nExcellent communication skills, with the ability to explain complex technical concepts and trade-offs clearly\r\nA collaborative mindset and experience mentoring other engineers in a distributed team environment\r\nTechnologies You’ll Work With\r\nBackend: Microsoft .NET, C#\r\nDatabases & Data Stores: MySQL, DynamoDB\r\nFrontend: JavaScript, HTML, CSS\r\nFrameworks: React, Angular\r\nCloud & Infrastructure: AWS, Terraform (or similar IaC tools)\r\nAPIs & Integrations: RESTful APIs, third-party and AI platform integrations\r\nAI: LLM-based workflows, Voice AI agents, AI-assisted development tools";
        var userSkills = new List<string> {".NET", "C#", "AWS", "MySQL", "LLMs", "Voice", "AI", "AI Coding Tools", "Terraform","CI/CD Pipelines", "Mentoring", "Technical Communication", "Agile Facilitation" };
        var sections = parser.GetSections(jobDescriptionReal);

        // Act
        var ranking = rankingEngine.CalcularRanking(sections, userSkills);

        // Assert
        // We expect a high ranking because the user has many of the required skills
        Assert.IsTrue(ranking > 50, $"Expected ranking to be greater than 50, but got {ranking}");  // this is a somewhat arbitrary threshold,
                                                                                                    // but we can adjust it based on the actual
                                                                                                    // scoring logic in the RankingEngine
    }
}
