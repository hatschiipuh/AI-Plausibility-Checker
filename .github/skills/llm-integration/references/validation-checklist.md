# Validation Checklist

Use this checklist whenever LLM output is processed by code.

## Required checks
- Is the payload syntactically valid?
- Does it match the expected structure?
- Are all required fields present?
- Are field types correct?
- Are enum values valid?
- Are null values allowed where they appear?
- Are numeric ranges valid?
- Are nested objects and arrays structurally correct?

## Defensive checks
- Is the response empty?
- Is the model refusing, hedging, or returning unrelated text?
- Did the provider return partial output?
- Are citations, URLs, or identifiers malformed?
- Is fallback parsing being triggered unexpectedly?

## Failure handling
- Reject invalid payloads clearly.
- Log enough detail for diagnosis without leaking sensitive data.
- Avoid silently coercing invalid values unless that behavior is intentional and tested.
- Prefer explicit error states over hidden degradation.

## Testing recommendations
- Add tests for valid structured output.
- Add tests for invalid JSON.
- Add tests for missing required fields.
- Add tests for wrong enum values.
- Add tests for empty or truncated responses.
- Add tests for fallback behavior if fallback parsing exists.
