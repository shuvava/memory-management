///
/// to build run g++ auto_memory.cpp -o ../../build/auto_memory
///
#include <iostream>
#include <memory>

void printReport(std::shared_ptr<int> data)
{
    std::cout << "Report: " << *data << "\n";
}

int main() {
    try {
        // shared_ptr that realizes reference counting semantics.
        std::shared_ptr<int> ptr(new int());
        *ptr = 25;
        printReport(ptr);
        return 0;
    }
    catch (std::bad_alloc& ba) {
        std::cout << "ERROR: Out of memory\n";
        return 1;
    }
}
