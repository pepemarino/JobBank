using JobBank.Management;

namespace WORKSCommons.Tests;


/// <summary>
/// Parsing Job Description seems frigale
/// </summary>
[TestClass]
public class JobDescriptionParserTest
{
    private JobDescriptionParser parser;
    private string jobDescription = "We are looking for a software engineer with experience in C# and .NET. The ideal candidate should have at least 5 years of experience in software development and be proficient in agile methodologies. Responsibilities include developing and maintaining software applications, collaborating with cross-functional teams, and participating in code reviews. The candidate should also have strong problem-solving skills and the ability to work in a fast-paced environment.";
    private string jobDescriptionWithTwoSections = "We are looking for a software engineer with experience in C# and .NET.\n\nRequirements:\n- At least 5 years of experience in software development\n- Proficiency in agile methodologies\n\nResponsibilities:\n- Developing and maintaining software applications\n- Collaborating with cross-functional teams\n- Participating in code reviews\n\nDesirable:\n- Strong problem-solving skills\n- Ability to work in a fast-paced environment";
    private string jobDescriptionWithBulletPoints = "We are looking for a software engineer with experience in C# and .NET. The ideal candidate should have at least 5 years of experience in software development and be proficient in agile methodologies. Responsibilities include:\n- Developing and maintaining software applications\n- Collaborating with cross-functional teams\n- Participating in code reviews\nThe candidate should also have strong problem-solving skills and the ability to work in a fast-paced environment.";
    private string jobDescriptionWithMixedFormats = "We are looking for a software engineer with experience in C# and .NET.\n\nRequirements:\n- At least 5 years of experience in software development\n- Proficiency in agile methodologies\nResponsibilities include:\n- Developing and maintaining software applications\n- Collaborating with cross-functional teams\n- Participating in code reviews\nThe candidate should also have strong problem-solving skills and the ability to work in a fast-paced environment.";
    private string jobDescriptionWithIrrelevantInformation = "We are looking for a software engineer with experience in C# and .NET. The ideal candidate should have at least 5 years of experience in software development and be proficient in agile methodologies. Responsibilities include developing and maintaining software applications, collaborating with cross-functional teams, and participating in code reviews. The candidate should also have strong problem-solving skills and the ability to work in a fast-paced environment. Note: This job is located in Toronto.";
    private string jobDescriptionReal = "What You’ll Do\r\nDesign, build, and maintain scalable backend services and APIs using .NET and C#\r\nLead the development of LLM-powered workflows and Voice AI agents used in production healthcare systems\r\nUse AI-assisted tools daily to support coding, code reviews, documentation, system design, and problem-solving\r\nParticipate in architecture discussions and design reviews, ensuring systems meet quality, scalability, security, and compliance standards\r\nOwn services end-to-end, including feature development, bug fixes, performance improvements, and technology upgrades\r\nIntegrate internal services, third-party APIs, and AI platforms via secure, reliable RESTful APIs\r\nBuild and improve CI/CD pipelines and contribute to infrastructure design using infrastructure-as-code\r\nSupport production systems, help resolve escalations, and proactively identify operational risks\r\nCollaborate closely with Product Management, Operations, and other engineering teams in an agile environment\r\nMentor and support other engineers, setting best practices for system design, code quality, and responsible AI-assisted development\r\nContribute to sprint planning, estimation, execution, retrospectives, and occasional facilitation of agile ceremonies\r\n\r\nWhat You Bring\r\nBachelor’s degree in Computer Science or a related field, or equivalent practical experience\r\n6+ years of experience building, scaling, and supporting distributed systems, with strong hands-on experience in C# / .NET (or Java)\r\n2+ years of experience designing, building, or integrating LLM-based systems and/or Voice AI agents in production\r\nProven experience using AI tools as part of everyday engineering work (coding, documentation, design, and technical reasoning)\r\nExperience building client-side applications using React, Angular, or similar frameworks\r\nExperience implementing and maintaining CI/CD pipelines\r\nExperience designing and operating systems on AWS, including infrastructure-as-code (e.g., Terraform)\r\nStrong understanding of software quality, security, reliability, and operational best practices\r\nExcellent communication skills, with the ability to explain complex technical concepts and trade-offs clearly\r\nA collaborative mindset and experience mentoring other engineers in a distributed team environment\r\nTechnologies You’ll Work With\r\nBackend: Microsoft .NET, C#\r\nDatabases & Data Stores: MySQL, DynamoDB\r\nFrontend: JavaScript, HTML, CSS\r\nFrameworks: React, Angular\r\nCloud & Infrastructure: AWS, Terraform (or similar IaC tools)\r\nAPIs & Integrations: RESTful APIs, third-party and AI platform integrations\r\nAI: LLM-based workflows, Voice AI agents, AI-assisted development tools";
    private string jobDescriptionMustHave = "We are looking for a software engineer with experience in C# and .NET. The ideal candidate should have at least 5 years of experience in software development and be proficient in agile methodologies. Responsibilities include developing and maintaining software applications, collaborating with cross-functional teams, and participating in code reviews. The candidate should also have strong problem-solving skills and the ability to work in a fast-paced environment. Requirements: At least 5 years of experience in software development, Proficiency in agile methodologies.\r\nMust have AZURE, React, WCF and OAuth.";
    private string jobDescriptionWithAbout = "About the Role:\r\n\r\nAs a Senior AI Native Software Engineer, you will build customer-facing products, internal tools, and workflow automations by directing AI agents to accelerate software development. You will operate as a hands-on builder who starts from a customer or operational problem, translates it into clear specifications, orchestrates AI tools to produce robust solutions, and ensures what ships meets Remitly's standards for security, privacy, reliability, and quality.\r\n\r\nIn this role, you are expected to independently drive ambiguous problem spaces, shape technical patterns for the team, and raise the overall effectiveness of AI-native software development. You will also help define best practices for prompt design, agent orchestration, evaluation, and the responsible adoption of AI-native development workflows.\r\n\r\nYou Will:\r\n\r\nBuild and ship complete applications, workflow automations, and AI-powered tools within your team's product area using AI coding agents and related development tooling\r\nTranslate ambiguous customer and business needs into structured requirements, technical plans, and production-ready solutions\r\nOwn delivery from concept through deployment, including architecture, implementation, instrumentation, testing, rollout, and iteration\r\nEvaluate AI-generated code and artifacts against Remitly's standards for security, privacy, reliability, maintainability, and product quality before shipping\r\nPartner across engineering, product, design, data, and platform teams to deliver scalable end-to-end solutions\r\nUse metrics and analytics to assess adoption, quality, and business outcomes, and drive iterative improvements based on data\r\nMentor other engineers in AI-native development practices and contribute reusable approaches, documentation, and team standards\r\nYou Have:\r\n\r\n8+ years of software engineering experience building and shipping production systems, products, or platforms\r\n1+ years of demonstrated proficiency using AI coding agents or similar generative AI development tools for end-to-end application delivery\r\nStrong ability to decompose problems, write effective prompts, evaluate outputs critically, and iterate until solutions meet quality expectations\r\nSolid understanding of full-stack application development, including frontend, backend, APIs, data stores, and cloud infrastructure\r\nExperience with common architectural patterns such as web applications, service-oriented systems, databases, queues, and event-driven workflows\r\nStrong product sense and the ability to connect customer problems to practical, high-impact technical solutions\r\nStrong communication and collaboration skills, with sound judgment around secure and responsible software delivery.";
    private string jobDescriptionUppercase = "REQUIREMENTS:\n- 5+ years C#\nMUST HAVE: Azure\n";
    private string jobDescriptionFalsePositive = "We require strong communication skills and experience in Azure. The candidate must have experience in distributed systems.";

    public JobDescriptionParserTest()
    {
        parser = new JobDescriptionParser();
    }

    [TestMethod]
    public void ProcessJobDescriptionWithUppercaseSectionsTest()
    {
        // Act
        var result = parser.GetSections(jobDescriptionUppercase);

        // Assert
        Assert.IsTrue(result.Keys.Contains("Mandatory"));
        Assert.IsTrue(result["Mandatory"].Contains("Azure"));
    }

    [TestMethod]
    public void ProcessjobDescriptionWithAboutTest()
    {
        // Act
        var result = parser.GetSections(jobDescriptionWithAbout);

        // Assert
        Assert.IsTrue(result.Keys.Contains("Mandatory"));
        Assert.IsTrue(result["Mandatory"].Contains("1+ years of demonstrated proficiency using AI coding agents"));
    }

    [TestMethod]
    public void ProcessJobDescriptionWithFalsePositiveTest()
    {
        // Act
        var result = parser.GetSections(jobDescriptionFalsePositive);

        // Assert
        Assert.IsTrue(result.Keys.Contains("General"));
        Assert.IsTrue(result.Count == 1);
    }

    [TestMethod]
    public void ProcessJobDescriptionWithTwoSectionsTest()
    {
        // Act
        var result = parser.GetSections(jobDescriptionWithTwoSections);

        // Assert
        Assert.IsTrue(result.Keys.Contains("General"));
        Assert.IsTrue(result.Keys.Contains("Mandatory"));
        Assert.IsTrue(result.Keys.Contains("Responsibilities"));
        Assert.IsTrue(result.Keys.Contains("Preferable"));
        Assert.IsTrue(result.Count == 4);
    }

    [TestMethod]
    public void ProcessJobDescriptionWithBulletPointsTest()
    {
        // Act
        var result = parser.GetSections(jobDescriptionWithBulletPoints);

        // Assert
        Assert.IsTrue(result.Keys.Contains("General"));
        Assert.IsTrue(result.Count == 1);
    }

    [TestMethod]
    public void ProcessJobDescriptionWithMixedFormatsTest()
    {
        // Act
        var result = parser.GetSections(jobDescriptionWithMixedFormats);

        // Assert
        Assert.IsTrue(result.Keys.Contains("General"));
        Assert.IsTrue(result.Keys.Contains("Mandatory"));
      
        Assert.IsTrue(result.Count == 2);
    }

    [TestMethod]
    public void ProcessJobDescriptionWithIrrelevantInformationTest()
    {
        // Act
        var result = parser.GetSections(jobDescriptionWithIrrelevantInformation);

        // Assert
        Assert.IsTrue(result.Keys.Contains("General"));
        Assert.IsTrue(result.Count == 1);
    }

    [TestMethod]
    public void ProcessJobDescriptionWithoutSectionsTest()
    {
        // Act
        var result = parser.GetSections(jobDescription);

        // Assert
        Assert.IsTrue(result.Keys.Contains("General"));
        Assert.IsTrue(result.Count == 1);
    }

    [TestMethod]
    public void ProcessRealJobDescriptionTest()
    {
        // Act
        var result = parser.GetSections(jobDescriptionReal);

        // Assert
        Assert.IsTrue(result.Keys.Contains("General"));
        Assert.IsTrue(result.Keys.Contains("Mandatory"));

        Assert.IsTrue(result.Count == 2);
    }

    [TestMethod]
    public void ProcessJobDescriptionWithMustHaveSectionTest()
    {
        // Act
        var result = parser.GetSections(jobDescriptionMustHave);

        // Assert        
        Assert.IsTrue(result.Keys.Contains("General"));
        Assert.IsTrue(result.Count == 1);
    }
}

