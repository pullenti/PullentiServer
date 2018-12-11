
IMAGE = pullenti/pullenti-server

../PullentiNetCore:
	git clone https://github.com/pullenti/PullentiNetCore.git ../PullentiNetCore

EP.SdkCore: ../PullentiNetCore
	cp -r ../PullentiNetCore/EP.SdkCore .

image: EP.SdkCore
	docker build -t $(IMAGE) .

push:
	docker push pullenti/pullenti-server

deamon:
	docker run -d -p 8080:8080 $(IMAGE)

run:
	docker run -it --rm -p 8080:8080 $(IMAGE)

test:
	python test.py
