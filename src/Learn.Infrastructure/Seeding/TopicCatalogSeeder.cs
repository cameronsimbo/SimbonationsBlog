using Learn.Domain.Entities;
using Learn.Domain.Enums;
using Learn.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Learn.Infrastructure.Seeding;

public interface ITopicCatalogSeeder
{
    Task SeedAsync(CancellationToken cancellationToken = default);
}

public class TopicCatalogSeeder : ITopicCatalogSeeder
{
    private readonly LearnDbContext _db;

    public TopicCatalogSeeder(LearnDbContext db)
    {
        _db = db;
    }

    public async Task SeedAsync(CancellationToken cancellationToken = default)
    {
        if (await _db.Topics.AnyAsync(cancellationToken))
        {
            return;
        }

        SeedVietnamWar();
        SeedCFALevel1();

        await _db.SaveChangesAsync(cancellationToken);
    }

    private void SeedVietnamWar()
    {
        Topic topic = Topic.Create(
            "Vietnam War",
            "Explore the causes, key events, and lasting impact of the Vietnam War (1955–1975), one of the most significant conflicts of the Cold War era.",
            SubjectDomain.History,
            DifficultyLevel.Intermediate);

        topic.KeyConcepts = "Cold War context, Gulf of Tonkin, Tet Offensive, anti-war movement, fall of Saigon, geopolitical consequences, domino theory, guerrilla warfare, media coverage, diplomatic negotiations";
        topic.GenerationGuidance = "Focus on causes, strategies, political implications, and lasting impact. Avoid obscure trivia like individual soldier names or minor skirmish details. Emphasise analytical thinking — why events happened and what their consequences were.";
        topic.Publish();

        // Unit 1: Origins & Escalation
        Unit unit1 = Unit.Create(topic.Id, "Origins & Escalation", "French Indochina, Cold War tensions, and US involvement from advisors to ground troops.", 0);

        Lesson lesson1_1 = Lesson.Create(unit1.Id, "French Colonial Legacy", "The end of French Indochina and the roots of conflict.", 0, 8);
        lesson1_1.GenerationContext = "French colonisation of Vietnam, the First Indochina War, Dien Bien Phu (1954), Geneva Accords, partition at 17th parallel. Focus on how colonial legacy created conditions for the later conflict.";

        Lesson lesson1_2 = Lesson.Create(unit1.Id, "The Domino Theory & Early US Involvement", "How Cold War ideology drove American intervention.", 1, 8);
        lesson1_2.GenerationContext = "Eisenhower's domino theory, SEATO, US military advisors under Kennedy, Diem government, Strategic Hamlet Program. Focus on the ideological framework that justified escalation.";

        Lesson lesson1_3 = Lesson.Create(unit1.Id, "Gulf of Tonkin & Escalation", "The incident that transformed US involvement from advisory to combat.", 2, 8);
        lesson1_3.GenerationContext = "Gulf of Tonkin incident (1964), Gulf of Tonkin Resolution, Operation Rolling Thunder, deployment of ground troops. Focus on how a contested naval incident led to massive military escalation.";

        Lesson lesson1_review = Lesson.Create(unit1.Id, "Unit Review", "Consolidation session reviewing the key concepts from this unit.", 3, 10);
        lesson1_review.GenerationContext = "This is a review lesson. Generate questions that synthesise and compare concepts across the whole unit. Prioritise analytical and evaluative questions over factual recall. Cover any areas where a student might have gaps: French colonial legacy, domino theory, Gulf of Tonkin, US escalation from advisors to combat troops.";

        unit1.Lessons.Add(lesson1_1);
        unit1.Lessons.Add(lesson1_2);
        unit1.Lessons.Add(lesson1_3);
        unit1.Lessons.Add(lesson1_review);

        // Unit 2: Key Turning Points
        Unit unit2 = Unit.Create(topic.Id, "Key Turning Points", "Major events that shifted the course and perception of the war.", 1);

        Lesson lesson2_1 = Lesson.Create(unit2.Id, "The Tet Offensive", "The 1968 offensive that changed everything.", 0, 10);
        lesson2_1.GenerationContext = "Tet Offensive (January 1968), surprise attacks across South Vietnam, Battle of Hue, siege of Khe Sanh. Focus on the strategic significance: military failure but political victory for North Vietnam, watershed moment in American public opinion.";

        Lesson lesson2_2 = Lesson.Create(unit2.Id, "My Lai & Moral Reckoning", "War crimes and their impact on public support.", 1, 8);
        lesson2_2.GenerationContext = "My Lai massacre (1968), cover-up and exposure, trial of Lt. William Calley, broader pattern of civilian casualties. Focus on moral dimensions, impact on US public opinion and anti-war movement, rules of engagement failures.";

        Lesson lesson2_3 = Lesson.Create(unit2.Id, "Cambodian Incursion & Kent State", "Widening war and domestic crisis.", 2, 8);
        lesson2_3.GenerationContext = "Nixon's Cambodia incursion (1970), Kent State shootings, expansion of anti-war protest, congressional pushback. Focus on how military decisions abroad triggered political crisis at home.";

        Lesson lesson2_review = Lesson.Create(unit2.Id, "Unit Review", "Consolidation session reviewing the key concepts from this unit.", 3, 10);
        lesson2_review.GenerationContext = "This is a review lesson. Generate questions that synthesise and compare concepts across the whole unit. Prioritise analytical and evaluative questions over factual recall. Cover any areas where a student might have gaps: Tet Offensive strategic paradox, My Lai and moral accountability, Cambodia incursion and domestic backlash.";

        unit2.Lessons.Add(lesson2_1);
        unit2.Lessons.Add(lesson2_2);
        unit2.Lessons.Add(lesson2_3);
        unit2.Lessons.Add(lesson2_review);

        // Unit 3: Home Front & Media
        Unit unit3 = Unit.Create(topic.Id, "Home Front & Media", "How the war was perceived, debated, and protested at home.", 2);

        Lesson lesson3_1 = Lesson.Create(unit3.Id, "The Living Room War", "Television's unprecedented role in shaping public opinion.", 0, 8);
        lesson3_1.GenerationContext = "First televised war, Walter Cronkite's broadcast, credibility gap, role of photojournalism (napalm girl, Saigon execution). Focus on how media coverage eroded public support and changed the relationship between media and military.";

        Lesson lesson3_2 = Lesson.Create(unit3.Id, "The Anti-War Movement", "From teach-ins to mass demonstrations.", 1, 8);
        lesson3_2.GenerationContext = "Student movements, draft resistance, Vietnam Veterans Against the War, March on Pentagon, Chicago 1968 DNC protests. Focus on how the movement evolved, its strategies, and its actual impact on policy.";

        Lesson lesson3_3 = Lesson.Create(unit3.Id, "The Draft & Social Division", "How conscription deepened class and racial divides.", 2, 8);
        lesson3_3.GenerationContext = "Selective Service system, deferments and inequality, disproportionate impact on minorities and working class, draft lottery (1969), Muhammad Ali's refusal. Focus on social justice dimensions and how the draft system reflected broader societal inequities.";

        Lesson lesson3_review = Lesson.Create(unit3.Id, "Unit Review", "Consolidation session reviewing the key concepts from this unit.", 3, 10);
        lesson3_review.GenerationContext = "This is a review lesson. Generate questions that synthesise and compare concepts across the whole unit. Prioritise analytical and evaluative questions over factual recall. Cover any areas where a student might have gaps: media's role in shaping opinion, anti-war movement strategies and effectiveness, draft inequities and social divisions.";

        unit3.Lessons.Add(lesson3_1);
        unit3.Lessons.Add(lesson3_2);
        unit3.Lessons.Add(lesson3_3);
        unit3.Lessons.Add(lesson3_review);

        // Unit 4: Aftermath & Legacy
        Unit unit4 = Unit.Create(topic.Id, "Aftermath & Legacy", "The fall of Saigon, veterans' experience, and lasting geopolitical impact.", 3);

        Lesson lesson4_1 = Lesson.Create(unit4.Id, "Paris Peace Accords & Fall of Saigon", "The end of American involvement and reunification of Vietnam.", 0, 8);
        lesson4_1.GenerationContext = "Paris Peace Accords (1973), withdrawal of US forces, collapse of South Vietnam, Fall of Saigon (April 1975), evacuation of the US embassy. Focus on diplomatic processes, why the peace agreement failed, and the human cost of the final days.";

        Lesson lesson4_2 = Lesson.Create(unit4.Id, "Veterans & Reconciliation", "The experience of those who served and the long road to healing.", 1, 8);
        lesson4_2.GenerationContext = "PTSD recognition, Agent Orange health effects, Vietnam Veterans Memorial (1982), POW/MIA controversies, normalisation of US-Vietnam relations (1995). Focus on the human aftermath and societal processing of the conflict.";

        Lesson lesson4_3 = Lesson.Create(unit4.Id, "Lessons & Legacy", "How Vietnam reshaped American foreign policy and military strategy.", 2, 10);
        lesson4_3.GenerationContext = "Vietnam Syndrome, War Powers Act (1973), Powell Doctrine, all-volunteer force, impact on subsequent conflicts (Gulf War, Iraq/Afghanistan). Focus on institutional changes, the 'lessons of Vietnam' debate, and why this conflict remains politically and culturally significant.";

        Lesson lesson4_review = Lesson.Create(unit4.Id, "Unit Review", "Consolidation session reviewing the key concepts from this unit.", 3, 10);
        lesson4_review.GenerationContext = "This is a review lesson. Generate questions that synthesise and compare concepts across the whole unit. Prioritise analytical and evaluative questions over factual recall. Cover any areas where a student might have gaps: Paris Peace Accords failure, veterans' experiences and societal reintegration, lasting geopolitical and military doctrine changes.";

        unit4.Lessons.Add(lesson4_1);
        unit4.Lessons.Add(lesson4_2);
        unit4.Lessons.Add(lesson4_3);
        unit4.Lessons.Add(lesson4_review);

        // Unit 5: The Air War & Naval Operations
        Unit unit5 = Unit.Create(topic.Id, "The Air War & Naval Operations", "Strategic bombing campaigns, carrier operations, and environmental warfare.", 4);

        Lesson lesson5_1 = Lesson.Create(unit5.Id, "Rolling Thunder & Strategic Bombing", "The sustained bombing campaign against North Vietnam.", 0, 8);
        lesson5_1.GenerationContext = "Operation Rolling Thunder (1965–1968), targeting philosophy, restricted zones, bombing halt debates, effectiveness assessment. Focus on the strategic rationale, the political constraints on targeting, and why the campaign failed to break North Vietnamese resolve.";

        Lesson lesson5_2 = Lesson.Create(unit5.Id, "Carrier Operations in the Gulf of Tonkin", "The US Navy's role in projecting airpower.", 1, 8);
        lesson5_2.GenerationContext = "US Navy carrier task forces, Yankee Station and Dixie Station, carrier-launched strike missions, naval gunfire support, Gulf of Tonkin patrols. Focus on how naval aviation integrated with the broader air campaign and the logistical scale of sustained carrier operations.";

        Lesson lesson5_3 = Lesson.Create(unit5.Id, "Operation Ranch Hand & Defoliation", "Chemical herbicides and environmental warfare.", 2, 8);
        lesson5_3.GenerationContext = "Operation Ranch Hand, Agent Orange and dioxin contamination, defoliation of the Ho Chi Minh Trail, crop destruction programs, long-term health consequences for veterans and Vietnamese civilians. Focus on the strategic intent, ethical controversy, and enduring environmental and health legacy.";

        Lesson lesson5_review = Lesson.Create(unit5.Id, "Unit Review", "Consolidation session reviewing the key concepts from this unit.", 3, 10);
        lesson5_review.GenerationContext = "This is a review lesson. Generate questions that synthesise and compare concepts across the whole unit. Prioritise analytical and evaluative questions over factual recall. Cover any areas where a student might have gaps: strategic bombing effectiveness and political constraints, naval airpower projection, Agent Orange and the ethics of environmental warfare.";

        unit5.Lessons.Add(lesson5_1);
        unit5.Lessons.Add(lesson5_2);
        unit5.Lessons.Add(lesson5_3);
        unit5.Lessons.Add(lesson5_review);

        // Unit 6: Peace Negotiations & American Withdrawal
        Unit unit6 = Unit.Create(topic.Id, "Peace Negotiations & American Withdrawal", "The diplomacy, Vietnamization strategy, and final collapse.", 5);

        Lesson lesson6_1 = Lesson.Create(unit6.Id, "The Paris Peace Talks", "Years of diplomatic maneuvering before a ceasefire.", 0, 8);
        lesson6_1.GenerationContext = "Paris Peace Talks (1968–1973), key negotiators (Kissinger, Le Duc Tho), sticking points over POWs and territorial control, Christmas Bombing (Operation Linebacker II), Nobel Peace Prize controversy. Focus on why negotiations took five years, what each side wanted, and what the agreement actually achieved.";

        Lesson lesson6_2 = Lesson.Create(unit6.Id, "Vietnamization & Nixon Doctrine", "Shifting the war's burden to South Vietnamese forces.", 1, 8);
        lesson6_2.GenerationContext = "Nixon Doctrine, Vietnamization program, ARVN capability building, drawdown of US troops, South Vietnamese performance in Lam Son 719 (1971). Focus on evaluating whether Vietnamization was a viable strategy or a face-saving withdrawal, and why South Vietnam ultimately collapsed.";

        Lesson lesson6_3 = Lesson.Create(unit6.Id, "The Fall of Saigon", "The final collapse and its immediate aftermath.", 2, 10);
        lesson6_3.GenerationContext = "North Vietnamese Spring Offensive (1975), ARVN collapse, congressional refusal of emergency aid, Operation Frequent Wind evacuation, refugee crisis, fate of South Vietnamese allies left behind. Focus on the sequence of military and political events, the human tragedy, and what the fall meant for US credibility in the Cold War.";

        Lesson lesson6_review = Lesson.Create(unit6.Id, "Unit Review", "Consolidation session reviewing the key concepts from this unit.", 3, 10);
        lesson6_review.GenerationContext = "This is a review lesson. Generate questions that synthesise and compare concepts across the whole unit. Prioritise analytical and evaluative questions over factual recall. Cover any areas where a student might have gaps: Paris Peace Talks and why a lasting peace proved impossible, Vietnamization's strategic logic and failures, the Fall of Saigon and its Cold War significance.";

        unit6.Lessons.Add(lesson6_1);
        unit6.Lessons.Add(lesson6_2);
        unit6.Lessons.Add(lesson6_3);
        unit6.Lessons.Add(lesson6_review);

        topic.TotalUnits = 6;
        topic.Units.Add(unit1);
        topic.Units.Add(unit2);
        topic.Units.Add(unit3);
        topic.Units.Add(unit4);
        topic.Units.Add(unit5);
        topic.Units.Add(unit6);

        _db.Topics.Add(topic);
    }

    private void SeedCFALevel1()
    {
        Topic topic = Topic.Create(
            "CFA Level 1",
            "Master the foundational concepts of the CFA Program Level 1 curriculum — ethics, quantitative methods, economics, and financial analysis.",
            SubjectDomain.Finance,
            DifficultyLevel.Advanced);

        topic.KeyConcepts = "Ethics & Professional Standards, Quantitative Methods, Economics, Financial Statement Analysis, Corporate Issuers, Equity Investments, Fixed Income, Derivatives, Alternative Investments, Portfolio Management";
        topic.GenerationGuidance = "Frame questions as CFA exam-style: scenario-based, analytical reasoning, calculations where appropriate. Focus on conceptual understanding and practical application. Use realistic financial scenarios. Avoid simple recall — test application and analysis.";
        topic.Publish();

        // Unit 1: Ethics & Professional Standards
        Unit unit1 = Unit.Create(topic.Id, "Ethics & Professional Standards", "The ethical foundation of investment management — Code of Ethics and Standards of Professional Conduct.", 0);

        Lesson lesson1_1 = Lesson.Create(unit1.Id, "Code of Ethics & Standards Overview", "The CFA Institute Code of Ethics and seven Standards of Professional Conduct.", 0, 10);
        lesson1_1.GenerationContext = "Six components of the Code of Ethics, seven Standards of Professional Conduct (Professionalism, Integrity of Capital Markets, Duties to Clients, Duties to Employers, Investment Analysis, Conflicts of Interest, Responsibilities as CFA Member). Focus on understanding the principles and their application to real scenarios.";

        Lesson lesson1_2 = Lesson.Create(unit1.Id, "GIPS & Ethical Scenarios", "Global Investment Performance Standards and applying ethics to complex scenarios.", 1, 10);
        lesson1_2.GenerationContext = "GIPS overview, composite construction, performance presentation, verification. Ethical scenario analysis: insider trading, front-running, misrepresentation, duty of loyalty. Focus on applying standards to nuanced real-world situations.";

        Lesson lesson1_3 = Lesson.Create(unit1.Id, "Duty to Clients & Fair Dealing", "Fiduciary duty, suitability, and fair treatment of all clients.", 2, 8);
        lesson1_3.GenerationContext = "Fiduciary duty, suitability requirements, fair dealing vs equal treatment, soft dollar arrangements, priority of transactions. Focus on practical application — given a scenario, identify the appropriate ethical action.";

        Lesson lesson1_review = Lesson.Create(unit1.Id, "Unit Review", "Consolidation session reviewing the key concepts from this unit.", 3, 10);
        lesson1_review.GenerationContext = "This is a review lesson. Generate scenario-based questions that synthesise concepts across the whole unit. Prioritise analytical and evaluative questions over factual recall. Cover any areas where a student might have gaps: Code of Ethics components, Standards of Professional Conduct application, GIPS compliance, fiduciary duty and fair dealing in complex scenarios.";

        unit1.Lessons.Add(lesson1_1);
        unit1.Lessons.Add(lesson1_2);
        unit1.Lessons.Add(lesson1_3);
        unit1.Lessons.Add(lesson1_review);

        // Unit 2: Quantitative Methods
        Unit unit2 = Unit.Create(topic.Id, "Quantitative Methods", "Time value of money, statistics, and probability — the mathematical toolkit for finance.", 1);

        Lesson lesson2_1 = Lesson.Create(unit2.Id, "Time Value of Money", "Present value, future value, annuities, and perpetuities.", 0, 10);
        lesson2_1.GenerationContext = "PV, FV, annuities (ordinary and due), perpetuities, effective annual rate vs stated rate, compounding frequencies. Focus on setting up calculations correctly given different scenarios — mortgage payments, bond pricing, retirement planning.";

        Lesson lesson2_2 = Lesson.Create(unit2.Id, "Statistical Concepts & Probability", "Descriptive statistics, probability distributions, and hypothesis testing.", 1, 10);
        lesson2_2.GenerationContext = "Mean/median/mode, variance/standard deviation, normal distribution, z-scores, confidence intervals, hypothesis testing (null vs alternative), Type I and Type II errors, p-values. Focus on interpretation and application, not just formulas.";

        Lesson lesson2_3 = Lesson.Create(unit2.Id, "Regression & Correlation", "Understanding relationships between financial variables.", 2, 8);
        lesson2_3.GenerationContext = "Simple linear regression, R-squared, correlation coefficient, standard error, t-statistics, ANOVA. Focus on interpreting regression output, understanding limitations, and applying to financial analysis (e.g., beta estimation, factor models).";

        Lesson lesson2_review = Lesson.Create(unit2.Id, "Unit Review", "Consolidation session reviewing the key concepts from this unit.", 3, 10);
        lesson2_review.GenerationContext = "This is a review lesson. Generate calculation-based and interpretive questions that synthesise concepts across the whole unit. Prioritise applied problems over definitions. Cover any areas where a student might have gaps: TVM in multi-step scenarios, statistical inference, regression interpretation, and connecting these tools to investment decision-making.";

        unit2.Lessons.Add(lesson2_1);
        unit2.Lessons.Add(lesson2_2);
        unit2.Lessons.Add(lesson2_3);
        unit2.Lessons.Add(lesson2_review);

        // Unit 3: Financial Statement Analysis
        Unit unit3 = Unit.Create(topic.Id, "Financial Statement Analysis", "Reading, analysing, and interpreting financial statements for investment decisions.", 2);

        Lesson lesson3_1 = Lesson.Create(unit3.Id, "Income Statement Analysis", "Revenue recognition, expense matching, and profitability metrics.", 0, 10);
        lesson3_1.GenerationContext = "Revenue recognition principles (IFRS 15/ASC 606), expense recognition, gross/operating/net profit margins, EPS (basic and diluted), non-recurring items, common-size analysis. Focus on analytical interpretation — what do the numbers tell you about company performance?";

        Lesson lesson3_2 = Lesson.Create(unit3.Id, "Balance Sheet & Cash Flow", "Assets, liabilities, equity, and the statement of cash flows.", 1, 10);
        lesson3_2.GenerationContext = "Current vs non-current classification, working capital analysis, debt-to-equity, operating/investing/financing cash flows, free cash flow calculation, accrual vs cash accounting differences. Focus on connecting all three statements and identifying red flags.";

        Lesson lesson3_3 = Lesson.Create(unit3.Id, "Financial Ratios & Quality", "Ratio analysis framework and assessing earnings quality.", 2, 10);
        lesson3_3.GenerationContext = "Liquidity ratios (current, quick), profitability ratios (ROE, ROA, DuPont decomposition), leverage ratios, efficiency ratios (turnover), earnings quality indicators, Beneish M-Score concept. Focus on using ratios to make investment decisions and detect potential problems.";

        Lesson lesson3_review = Lesson.Create(unit3.Id, "Unit Review", "Consolidation session reviewing the key concepts from this unit.", 3, 10);
        lesson3_review.GenerationContext = "This is a review lesson. Generate scenario-based questions using provided financial data to synthesise concepts across the whole unit. Prioritise cross-statement analysis and ratio interpretation over isolated definitions. Cover any areas where a student might have gaps: revenue quality assessment, cash flow vs earnings divergence, DuPont decomposition, and earnings quality red flags.";

        unit3.Lessons.Add(lesson3_1);
        unit3.Lessons.Add(lesson3_2);
        unit3.Lessons.Add(lesson3_3);
        unit3.Lessons.Add(lesson3_review);

        // Unit 4: Equity & Fixed Income
        Unit unit4 = Unit.Create(topic.Id, "Equity & Fixed Income", "Valuation principles for stocks and bonds.", 3);

        Lesson lesson4_1 = Lesson.Create(unit4.Id, "Equity Valuation Models", "DDM, free cash flow models, and relative valuation.", 0, 10);
        lesson4_1.GenerationContext = "Gordon Growth Model, multi-stage DDM, FCFE/FCFF models, P/E ratio analysis, P/B, EV/EBITDA, justified multiples. Focus on selecting the appropriate model for a given company and understanding assumptions behind each approach.";

        Lesson lesson4_2 = Lesson.Create(unit4.Id, "Fixed Income Fundamentals", "Bond pricing, yield measures, and interest rate risk.", 1, 10);
        lesson4_2.GenerationContext = "Bond pricing formula, YTM, current yield, yield curve shapes, duration (Macaulay and modified), convexity, credit risk vs interest rate risk. Focus on understanding how bond prices move with interest rates and applying duration as a risk measure.";

        Lesson lesson4_3 = Lesson.Create(unit4.Id, "Portfolio Theory Basics", "Risk-return tradeoff, diversification, and the efficient frontier.", 2, 10);
        lesson4_3.GenerationContext = "Markowitz portfolio theory, expected return/variance of a portfolio, correlation and diversification benefits, efficient frontier, Capital Market Line, CAPM, systematic vs unsystematic risk, beta. Focus on why diversification works and how to think about risk at the portfolio level.";

        Lesson lesson4_review = Lesson.Create(unit4.Id, "Unit Review", "Consolidation session reviewing the key concepts from this unit.", 3, 10);
        lesson4_review.GenerationContext = "This is a review lesson. Generate CFA exam-style scenario questions that synthesise concepts across the whole unit. Prioritise model selection reasoning and risk measurement application. Cover any areas where a student might have gaps: choosing between DDM and FCF models, bond duration and convexity in price change estimation, CAPM assumptions and beta interpretation.";

        unit4.Lessons.Add(lesson4_1);
        unit4.Lessons.Add(lesson4_2);
        unit4.Lessons.Add(lesson4_3);
        unit4.Lessons.Add(lesson4_review);

        // Unit 5: Derivatives & Alternative Investments
        Unit unit5 = Unit.Create(topic.Id, "Derivatives & Alternative Investments", "Forwards, futures, options, swaps, and the landscape of alternative asset classes.", 4);

        Lesson lesson5_1 = Lesson.Create(unit5.Id, "Futures & Forwards Fundamentals", "Pricing, hedging, and speculating with derivative contracts.", 0, 10);
        lesson5_1.GenerationContext = "Forward contracts vs futures (standardisation, margin, mark-to-market), cost of carry pricing model, basis risk, hedging with futures (currency, commodity, equity index), contango vs backwardation. Focus on calculating forward prices and understanding hedge ratios in practical scenarios.";

        Lesson lesson5_2 = Lesson.Create(unit5.Id, "Options Pricing & Greeks", "Understanding option value and its sensitivities.", 1, 10);
        lesson5_2.GenerationContext = "Call and put payoff diagrams, intrinsic vs time value, Black-Scholes model inputs, put-call parity, option Greeks (delta, gamma, vega, theta, rho). Focus on intuitive understanding of what drives option prices and how Greeks measure sensitivity, with CFA-style scenario questions.";

        Lesson lesson5_3 = Lesson.Create(unit5.Id, "Swaps & Credit Derivatives", "Interest rate swaps, currency swaps, and credit default swaps.", 2, 8);
        lesson5_3.GenerationContext = "Plain vanilla interest rate swaps, fixed vs floating legs, swap valuation, currency swaps and FX risk management, credit default swaps (CDS) mechanics and credit risk transfer. Focus on why counterparties enter swaps, how cash flows are determined, and the role of CDS in the 2008 financial crisis.";

        Lesson lesson5_review = Lesson.Create(unit5.Id, "Unit Review", "Consolidation session reviewing the key concepts from this unit.", 3, 10);
        lesson5_review.GenerationContext = "This is a review lesson. Generate CFA exam-style scenario questions that synthesise concepts across the whole unit. Prioritise pricing calculations and hedging strategy analysis. Cover any areas where a student might have gaps: forward pricing vs futures settlement, option Greeks in portfolio management, swap cash flow determination and use cases.";

        unit5.Lessons.Add(lesson5_1);
        unit5.Lessons.Add(lesson5_2);
        unit5.Lessons.Add(lesson5_3);
        unit5.Lessons.Add(lesson5_review);

        // Unit 6: Portfolio Management & Risk
        Unit unit6 = Unit.Create(topic.Id, "Portfolio Management & Risk", "Constructing optimal portfolios and measuring risk-adjusted performance.", 5);

        Lesson lesson6_1 = Lesson.Create(unit6.Id, "Modern Portfolio Theory & Efficient Frontier", "Diversification, portfolio variance, and the efficient frontier.", 0, 10);
        lesson6_1.GenerationContext = "Markowitz mean-variance optimisation, two-asset portfolio variance formula, correlation's role in diversification, minimum variance portfolio, efficient frontier construction, investor utility and risk aversion. Focus on why adding low-correlation assets reduces portfolio risk and how to identify efficient portfolios.";

        Lesson lesson6_2 = Lesson.Create(unit6.Id, "Capital Asset Pricing Model", "Systematic risk, beta, and the Security Market Line.", 1, 10);
        lesson6_2.GenerationContext = "CAPM assumptions, Security Market Line (SML) vs Capital Market Line (CML), beta as systematic risk measure, expected return calculation, alpha (Jensen's alpha), portfolio beta. Focus on applying CAPM to price assets, identify undervalued/overvalued securities, and understand what CAPM does and does not capture.";

        Lesson lesson6_3 = Lesson.Create(unit6.Id, "Risk-Adjusted Performance Measures", "Evaluating portfolio performance beyond raw returns.", 2, 10);
        lesson6_3.GenerationContext = "Sharpe ratio, Treynor ratio, Jensen's alpha, information ratio, M-squared. When to use each measure (total vs systematic risk contexts), limitations of each metric, comparing portfolios with different risk profiles. Focus on selecting the appropriate measure given the investment mandate and interpreting results in CFA exam scenarios.";

        Lesson lesson6_review = Lesson.Create(unit6.Id, "Unit Review", "Consolidation session reviewing the key concepts from this unit.", 3, 10);
        lesson6_review.GenerationContext = "This is a review lesson. Generate CFA exam-style scenario questions that synthesise concepts across the whole unit and connect back to earlier units where appropriate. Prioritise portfolio construction decisions and performance evaluation over isolated formula recall. Cover any areas where a student might have gaps: efficient frontier and optimal portfolio selection, CAPM in asset pricing and security selection, choosing and interpreting risk-adjusted performance measures.";

        unit6.Lessons.Add(lesson6_1);
        unit6.Lessons.Add(lesson6_2);
        unit6.Lessons.Add(lesson6_3);
        unit6.Lessons.Add(lesson6_review);

        topic.TotalUnits = 6;
        topic.Units.Add(unit1);
        topic.Units.Add(unit2);
        topic.Units.Add(unit3);
        topic.Units.Add(unit4);
        topic.Units.Add(unit5);
        topic.Units.Add(unit6);

        _db.Topics.Add(topic);
    }
}
