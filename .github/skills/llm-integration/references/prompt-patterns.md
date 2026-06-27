# Prompt Patterns

Use these patterns when building prompts for provider-backed LLM integrations.

## Goals
- Be explicit about the task.
- Be explicit about the expected output format.
- Be explicit about uncertainty handling.
- Keep prompts readable and stable over time.

## Recommended structure
1. State the role or task clearly.
2. Provide only the necessary context.
3. Define the required output format explicitly.
4. Define failure behavior or uncertainty behavior.
5. Keep examples short and only add them when they improve reliability.

## Good patterns
- "Return valid JSON matching this structure."
- "If the information is missing or uncertain, return `unknown`."
- "Do not infer fields that are not supported by the input."
- "Use the following categories only: ..."

## Avoid
- Long narrative prompts with hidden requirements
- Weak output wording like "prefer JSON" instead of "return valid JSON"
- Mixing business rules, parsing rules, and UI wording in one large prompt
- Depending on formatting habits that are not validated

## Prompt building guidance
- Prefer templates with clearly named placeholders over ad-hoc string concatenation.
- Build prompt inputs from validated application data.
- Keep system-level rules separate from user-provided content.
- Treat user-provided text as data, not as instruction authority.
