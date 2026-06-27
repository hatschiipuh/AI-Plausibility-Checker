---
applyTo: "**/frontend/**/*,**/src/app/**/*,**/*.ts,**/*.html,**/*.scss,**/*.css"
---

# Frontend Instructions

You are working on the frontend of this repository, including TypeScript, Angular, HTML, and CSS/SCSS.

## Communication
- Communicate with the user in **German** by default.
- Keep code, identifiers, filenames, component names, and technical terms in **English**, unless explicitly requested otherwise.
- Explanations, suggestions, and architectural discussions should be written in **German**.

## Core Principles
- Prefer simple, maintainable, and consistent frontend solutions.
- Follow the existing frontend architecture and project conventions instead of inventing new patterns.
- Point out code smells, UI architecture issues, and maintainability problems proactively.
- Prefer sustainable frontend architecture over short-term fixes.

## TypeScript
- Prefer strict typing and avoid `any` unless absolutely necessary.
- Use explicit interfaces, types, and clear models.
- Prefer small, focused functions and strongly typed APIs.
- Avoid fragile assumptions and unclear null or union handling.

## Angular
- Follow Angular best practices and the existing project conventions.
- Prefer small, focused, reusable components.
- Keep business logic out of components where possible.
- Avoid logic-heavy templates.
- Respect the existing structure for components, services, facades, models, and state handling.
- Do not introduce new architectural patterns unless there is a clear reason.

## HTML
- Prefer semantic HTML elements over generic `div` structures where possible.
- Maintain a clear heading hierarchy and accessible markup.
- Use accessible labels, alt text, button semantics, and keyboard-friendly interactions.
- Avoid unnecessary wrapper elements and overly nested markup.

## CSS and SCSS
- Prefer existing styling conventions and reuse existing classes, variables, mixins, or design tokens.
- Avoid inline styles unless they are truly dynamic and justified.
- Keep styles modular, readable, and scoped appropriately.
- Prefer clean layout systems and responsive styling over one-off fixes.
- Avoid duplicated styling rules and magic values where reusable tokens or variables exist.

## UI and Accessibility
- Consider responsive behavior by default.
- Preserve usability on smaller screens and common viewport sizes.
- Prefer accessible color contrast, focus states, and readable spacing.
- Point out accessibility issues when they are visible in the code or markup.

## Code Quality
- Write production-oriented, maintainable, and testable frontend code.
- Avoid oversized components, duplicated logic, weak typing, and hard-to-test code.
- Update or suggest tests when behavior changes.
- If a quick fix increases coupling or complexity, explain that clearly and propose the cleaner option first.
