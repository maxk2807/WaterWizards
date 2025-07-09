import os
import re
import subprocess
from collections import Counter

SRC_DIR = 'src'
EXCLUDE_DIRS = {'obj', 'bin'}

METHOD_REGEX = re.compile(r'^(\s*(public|private|protected|internal)[^;{]*\s+([a-zA-Z0-9_<>]+)\s*\([^)]*\)\s*[{;])', re.MULTILINE)
BLOCK_REGEX = re.compile(r'^(// =+\n(?:.|\n)*?// =+\n)', re.MULTILINE)

def find_cs_files():
    cs_files = []
    for root, dirs, files in os.walk(SRC_DIR):
        dirs[:] = [d for d in dirs if d not in EXCLUDE_DIRS]
        for file in files:
            if file.endswith('.cs'):
                cs_files.append(os.path.join(root, file))
    return cs_files

def get_file_authors(filename):
    result = subprocess.run(['git', 'blame', '--line-porcelain', filename], capture_output=True, text=True)
    authors = [line[7:] for line in result.stdout.splitlines() if line.startswith('author ')]
    return Counter(authors)

def get_methods_with_lines(filename):
    with open(filename, encoding='utf-8') as f:
        lines = f.readlines()
    methods = []
    for i, line in enumerate(lines):
        m = METHOD_REGEX.match(line)
        if m:
            method_name = m.group(1).strip()
            start = i
            end = len(lines)
            for j in range(i+1, len(lines)):
                if METHOD_REGEX.match(lines[j]):
                    end = j
                    break
            methods.append((method_name, start, end))
    return methods

def get_method_authors(filename, start, end):
    result = subprocess.run(['git', 'blame', '--line-porcelain', filename], capture_output=True, text=True)
    blame_lines = result.stdout.splitlines()
    authors = []
    line_idx = -1
    for line in blame_lines:
        if line.startswith('author '):
            author = line[7:]
        if line.startswith('author '):
            line_idx += 1
            if start <= line_idx < end:
                authors.append(author)
    return Counter(authors)

def build_comment_block(file_authors, methods_info):
    comment = '// ===============================================\n'
    comment += '// Autoren-Statistik (automatisch generiert):\n'
    for author, count in file_authors.most_common():
        comment += f'// - {author}: {count} Zeilen\n'
    comment += '// \n// Methoden/Funktionen in dieser Datei (Hauptautor):\n'
    if not methods_info:
        comment += '// (Keine Methoden/Funktionen gefunden)\n'
    else:
        for method, author, count in methods_info:
            comment += f'// - {method}   ({author}: {count} Zeilen)\n'
    comment += '// ===============================================\n'
    return comment

def insert_comment(filename, comment):
    with open(filename, encoding='utf-8') as f:
        content = f.read()
    head = content[:1000]
    match = BLOCK_REGEX.match(head)
    if match:
        content = BLOCK_REGEX.sub(comment + '\n', content, count=1)
    else:
        content = comment + '\n' + content
    with open(filename, 'w', encoding='utf-8') as f:
        f.write(content)

def main():
    for file in find_cs_files():
        print(f'Bearbeite {file} ...')
        file_authors = get_file_authors(file)
        methods = get_methods_with_lines(file)
        methods_info = []
        for method, start, end in methods:
            method_authors = get_method_authors(file, start, end)
            if method_authors:
                main_author, main_count = method_authors.most_common(1)[0]
                methods_info.append((method, main_author, main_count))
            else:
                methods_info.append((method, '-', 0))
        comment = build_comment_block(file_authors, methods_info)
        insert_comment(file, comment)
        print(f'Kommentarblock eingefügt in {file}')
    print('Fertig!')

if __name__ == '__main__':
    main() 