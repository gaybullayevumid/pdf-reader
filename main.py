from pathlib import Path


roadmap = {
    "01-javascript-basics": [
        "01-variables",
        "02-data-types",
        "03-operators",
        "04-type-conversion",
        "05-input-output",
    ],

    "02-control-flow": [
        "01-if-else",
        "02-switch",
        "03-ternary-operator",
        "04-truthy-vs-falsy",
    ],

    "03-loops": [
        "01-for",
        "02-while",
        "03-do-while",
        "04-break",
        "05-continue",
    ],

    "04-functions": [
        "01-function-declaration",
        "02-function-expression",
        "03-arrow-functions",
        "04-parameters-arguments",
        "05-return",
        "06-default-parameters",
        "07-rest-parameters",
        "08-callback-functions",
        "09-higher-order-functions",
    ],

    "05-strings": [
        "01-string-methods",
        "02-template-literals",
        "03-string-searching",
        "04-string-manipulation",
    ],

    "06-arrays": [
        "01-array-basics",
        "02-push-pop",
        "03-shift-unshift",
        "04-slice-splice",
        "05-foreach",
        "06-map",
        "07-filter",
        "08-reduce",
        "09-find",
        "10-some-every",
        "11-destructuring",
        "12-spread-operator",
    ],

    "07-objects": [
        "01-object-basics",
        "02-nested-objects",
        "03-object-methods",
        "04-this",
        "05-destructuring",
        "06-optional-chaining",
        "07-object-keys-values-entries",
    ],

    "08-advanced-functions": [
        "01-scope",
        "02-closures",
        "03-hoisting",
        "04-execution-context",
        "05-lexical-environment",
        "06-call",
        "07-apply",
        "08-bind",
    ],

    "09-dom": [
        "01-dom-introduction",
        "02-selecting-elements",
        "03-changing-content",
        "04-changing-styles",
        "05-creating-elements",
        "06-removing-elements",
        "07-traversing",
        "08-events",
        "09-event-listeners",
        "10-event-bubbling",
        "11-event-delegation",
    ],

    "10-bom": [
        "01-window",
        "02-location",
        "03-history",
        "04-navigator",
        "05-settimeout",
        "06-setinterval",
    ],

    "11-es6-plus": [
        "01-let-vs-var",
        "02-template-literals",
        "03-destructuring",
        "04-spread-rest",
        "05-modules",
        "06-classes",
        "07-promises",
        "08-async-await",
        "09-optional-chaining",
        "10-nullish-coalescing",
    ],

    "12-asynchronous-javascript": [
        "01-callbacks",
        "02-callback-hell",
        "03-promises",
        "04-promise-methods",
        "05-async-await",
        "06-event-loop",
        "07-microtask-vs-macrotask",
        "08-fetch-api",
        "09-json",
    ],

    "13-error-handling": [
        "01-try-catch",
        "02-throw",
        "03-custom-errors",
        "04-debugging",
        "05-strict-mode",
    ],

    "14-oop": [
        "01-constructor-functions",
        "02-prototypes",
        "03-prototype-chain",
        "04-classes",
        "05-inheritance",
        "06-encapsulation",
        "07-polymorphism",
        "08-static-methods",
        "09-private-fields",
    ],
}


current_path = Path.cwd()


for category, topics in roadmap.items():

    category_path = current_path / category
    category_path.mkdir(exist_ok=True)

    for topic in topics:

        topic_path = category_path / topic
        topic_path.mkdir(exist_ok=True)

        readme = topic_path / "README.md"

        if not readme.exists():
            title = topic.replace("-", " ").title()

            readme.write_text(
                f"# {title}\n\n",
                encoding="utf-8"
            )

print("Raqamlangan JavaScript roadmap folderlari yaratildi!")